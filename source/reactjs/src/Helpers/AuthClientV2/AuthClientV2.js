var __awaiter =
    (this && this.__awaiter) ||
    function (thisArg, _arguments, P, generator) {
        function adopt(value) {
            return value instanceof P
                ? value
                : new P(function (resolve) {
                      resolve(value);
                  });
        }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) {
                try {
                    step(generator.next(value));
                } catch (e) {
                    reject(e);
                }
            }
            function rejected(value) {
                try {
                    step(generator['throw'](value));
                } catch (e) {
                    reject(e);
                }
            }
            function step(result) {
                result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected);
            }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
import { PublicClientApplication, BrowserAuthError, InteractionRequiredAuthError } from '@azure/msal-browser';
export class AuthClientV2 {
    constructor(config, telemetryClient, options) {
        this.account = null;
        // Tracks whether there has been another login request during login redirect
        // If another login is called before login redirect is completed,
        // MSAL will throw interaction_in_progress exception
        this.isLoginRequested = false;
        // Tracks whether the login redirect has been completed
        this.isRedirectComplete = false;
        // Tracks all acquireTokens requests made during login redirection
        this.acquireTokenRequests = {};
        // Tracks all getUser requests made during login redirection
        this.getUserRequests = [];
        this.telemetryClient = telemetryClient;
        this.config = config;
        this.authContext = new PublicClientApplication({
            auth: config.auth,
            cache: Object.assign({ cacheLocation: 'sessionStorage' }, config.cache),
        });
        this.options = options || {};
        //remove session storage item if it exists to avoid showing session expired dialog on page load
        window.sessionStorage.setItem('isSessionExpired', 'false');
    }
    // Initialize the MSAL client and handle any redirect responses for msal v3 migration
    async initializeClient() {
        await this.authContext.initialize();
        this.authContext.handleRedirectPromise().then(this.handleRedirectCompleted.bind(this));
    }
    login(loginOptions = {}) {
        return new Promise((resolve, reject) =>
            __awaiter(this, void 0, void 0, function* () {
                var _a, _b;
                if (this.options.onLogin) this.options.onLogin();
                try {
                    if (this.isRedirectComplete) {
                        yield this.authContext.loginRedirect({
                            scopes:
                                (_a =
                                    loginOptions === null || loginOptions === void 0 ? void 0 : loginOptions.scopes) !==
                                    null && _a !== void 0
                                    ? _a
                                    : [],
                        });
                    } else {
                        this.isLoginRequested = true;
                    }
                    resolve();
                } catch (ex) {
                    if (this.options.onLoginFailed) this.options.onLoginFailed();
                    if (ex instanceof BrowserAuthError) {
                        sessionStorage.clear();
                        yield this.authContext.loginRedirect({
                            scopes:
                                (_b =
                                    loginOptions === null || loginOptions === void 0 ? void 0 : loginOptions.scopes) !==
                                    null && _b !== void 0
                                    ? _b
                                    : [],
                        });
                    } else {
                        reject(ex);
                    }
                }
            })
        );
    }
    async loginPopup(loginOptions = {}) {
        var _a;
        try {
            const request = {
                scopes:
                    (_a =
                        loginOptions === null || loginOptions === void 0 ? void 0 : loginOptions.scopes) !==
                        null && _a !== void 0
                        ? _a
                        : [],
                correlationId: this.telemetryClient.getCorrelationId(),

            };
            if (loginOptions.prompt) {
                request.prompt = loginOptions.prompt;
            }
            const response = await this.authContext.loginPopup(request);
            if (response && response.account) {
                this.authContext.setActiveAccount(response.account);
                this.account = response.account;
            }
            return response;
        } catch (ex) {
            console.log('Error during popup login', ex);
            if (this.options.onLoginFailed) this.options.onLoginFailed();
            throw ex;
        }
    }
    logOut() {
        return new Promise((resolve, reject) =>
            __awaiter(this, void 0, void 0, function* () {
                if (this.options.onLogout) this.options.onLogout();
                try {
                    yield this.authContext.logoutRedirect({
                        account: this.account,
                    });
                    resolve();
                } catch (ex) {
                    if (this.options.onLogoutFailed) this.options.onLogoutFailed();
                    reject(ex);
                }
            })
        );
    }
    getUser() {
        return new Promise((resolve) =>
            __awaiter(this, void 0, void 0, function* () {
                if (this.isRedirectComplete) {
                    const user = yield this.getUserInner();
                    resolve(user);
                } else {
                    this.addGetUserRequest(resolve);
                }
            })
        );
    }
    getUserId() {
        return new Promise((resolve, reject) =>
            __awaiter(this, void 0, void 0, function* () {
                try {
                    const user = yield this.getUser();
                    resolve(user ? user.id : null);
                } catch (ex) {
                    reject(ex);
                }
            })
        );
    }
    isLoggedIn() {
        return new Promise((resolve, reject) =>
            __awaiter(this, void 0, void 0, function* () {
                try {
                    const user = yield this.getUser();
                    resolve(!!user);
                } catch (ex) {
                    reject(ex);
                }
            })
        );
    }
    acquireToken(scopes) {
        return new Promise((resolve, reject) =>
            __awaiter(this, void 0, void 0, function* () {
                if (this.isRedirectComplete && this.account) {
                    try {
                        const accessToken = yield this.acquireTokenSilent(scopes);
                        resolve(accessToken);
                    } catch (e) {
                        reject(e);
                    }
                } else {
                    //console.log('Login is not completed yet. Adding acquireToken request to queue.');
                    this.addAcquireTokenRequest(scopes, resolve);
                }
            })
        );
    }
    handleRedirectCompleted(response) {
        this.account = (response === null || response === void 0 ? void 0 : response.account) || this.getCachedUser();
        this.isRedirectComplete = true;
        // Trigger login only if login redirection completed without the account info
        // This means there was no login request perviously
        if (this.isLoginRequested && !this.account) {
            this.login().catch();
            return;
        }
        if (this.account) {
            this.flushAcquireTokenRequests();
        }
        this.flushGetUserRequests();
    }
    getUserInner() {
        return new Promise((resolve, reject) => {
            try {
                const user = this.getCachedUser();
                if (!user) {
                    resolve(null);
                    return;
                }
                if (this.options.onGetUser) {
                    resolve(this.options.onGetUser(user));
                    return;
                }
                resolve({
                    id: user.username,
                    email: user.username,
                    name: user.name || this.getNameFromIdToken(user.idTokenClaims) || '',
                    oid: user.homeAccountId,
                });
            } catch (ex) {
                reject(ex);
            }
        });
    }
    getCachedUser() {
        const activeAccount = this.authContext.getActiveAccount();
        if (activeAccount) return activeAccount;
        const users = this.authContext.getAllAccounts();
        if (!users || users.length === 0) return null;
        if (users.length === 1) {
            const selectedUser = users[0];
            this.authContext.setActiveAccount(selectedUser);
            return selectedUser;
        }
        if (users.length > 1) {
            if (this.options.onMultipleAccountFound) {
                const selectedUser = this.options.onMultipleAccountFound(users);
                if (selectedUser) {
                    this.authContext.setActiveAccount(selectedUser);
                    return selectedUser;
                }
            }
            throw new Error('MultipleAccountFound');
        }
    }
    normalizeScopes(scopes) {
        let normalizedScopes = [];
        if (typeof scopes === 'string') normalizedScopes.push(scopes + '/.default');
        else normalizedScopes = [...scopes];
        return normalizedScopes;
    }
    addGetUserRequest(callback) {
        this.getUserRequests.push(callback);
    }
    flushGetUserRequests() {
        this.getUserRequests.forEach((cb) => {
            this.getUserInner().then(cb);
        });
    }
    addAcquireTokenRequest(scopes, callback) {
        const normalizedScopes = this.normalizeScopes(scopes);
        const key = normalizedScopes.join(',');
        this.acquireTokenRequests[key] = this.acquireTokenRequests[key] || [];
        this.acquireTokenRequests[key].push(callback);
    }
    flushAcquireTokenRequests() {
        for (const key in this.acquireTokenRequests) {
            const scopes = key.split(',');
            this.acquireTokenSilent(scopes).then((accessToken) => {
                this.acquireTokenRequests[key].forEach((cb) => {
                    cb(accessToken);
                });
            });
        }
    }
    acquireTokenSilent(scopes) {
        return __awaiter(this, void 0, void 0, function* () {
            return new Promise((resolve, reject) => {
                var _a, _b;
                const normalizedScopes = this.normalizeScopes(scopes);
                this.authContext
                    .acquireTokenSilent({
                        authority: (_a = this.config.auth) === null || _a === void 0 ? void 0 : _a.authority,
                        scopes: normalizedScopes,
                        account: this.account,
                        correlationId: this.telemetryClient.getCorrelationId(),
                        redirectUri:
                            ((_b = this.config.auth) === null || _b === void 0 ? void 0 : _b.redirectUri) ||
                            window.location.origin,
                    })
                    .then(({ accessToken }) => {
                        if (accessToken) {
                            resolve(accessToken);
                            return;
                        }
                        // Azure B2C does not return access token if login did not return access token for the scope requested.
                        // Throw exception to trigger acquireTokenRedirect
                        throw new Error('NoAccessTokenReceived');
                    })
                    .catch((e) => {
                        console.log('Error acquiring token silently', e);
                        var _a, _b;
                        if (this.options.onAcquireTokenError) {
                            this.options.onAcquireTokenError(e, scopes);
                        } else {
                            if (e instanceof InteractionRequiredAuthError || e instanceof BrowserAuthError) {
                                // storing expiration flag in session storage to be used by SessionExpiryDialog component based on silent token error
                                window.sessionStorage.setItem('isSessionExpired', 'true');
                                // disabling refresh without dialog shown to user
                                /*
                                this.authContext.acquireTokenRedirect({
                                    authority:
                                        (_a = this.config.auth) === null || _a === void 0 ? void 0 : _a.authority,
                                    scopes: normalizedScopes,
                                    correlationId: this.telemetryClient.getCorrelationId(),
                                    redirectUri:
                                        ((_b = this.config.auth) === null || _b === void 0 ? void 0 : _b.redirectUri) ||
                                        window.location.origin,
                                });
                                */
                            }
                        }
                        reject(e);
                    });
            });
        });
    }
    getNameFromIdToken(idTokenClaim) {
        if (!idTokenClaim) return '';
        if (idTokenClaim.family_name && idTokenClaim.given_name) {
            return `${idTokenClaim.given_name} ${idTokenClaim.family_name}`;
        }
        return '';
    }
}
//# sourceMappingURL=AuthClient.js.map
