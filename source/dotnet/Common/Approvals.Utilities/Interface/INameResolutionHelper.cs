// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Interface;

using System;
using System.Threading.Tasks;

public interface INameResolutionHelper
{
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <returns></returns>
    Task<Microsoft.Graph.Models.User> GetUser(string alias);

    /// <summary>
    /// Gets user by mail.
    /// </summary>
    /// <param name="mail"></param>
    /// <returns></returns>
    Task<Microsoft.Graph.Models.User> GetUserByMail(string mail);

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <returns></returns>
    Task<string> GetUserName(string alias);

    /// <summary>
    /// Get User Image.
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    Task<byte[]> GetUserImage(string alias);

    /// <summary>
    /// Check if User is valid
    /// </summary>
    /// <param name="Alias"></param>
    /// <returns></returns>
    Task<Tuple<bool, string>> IsValidUser(string Alias);

    /// <summary>
    /// Gets Id of the given alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    Task<string> GetObjectId(string alias);

    /// <summary>
    /// Gets UserPrincipalName of the given alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    Task<string> GetUserPrincipalName(string alias);

    /// <summary>
    /// Retrieves the Graph user associated with the specified object ID.
    /// </summary>
    /// <param name="userObjectId">The unique identifier of the user object. This parameter cannot be null or empty.</param>
    /// <returns>
    Task<Microsoft.Graph.Models.User>GetUserManagerId(string aliasObjectId);


}