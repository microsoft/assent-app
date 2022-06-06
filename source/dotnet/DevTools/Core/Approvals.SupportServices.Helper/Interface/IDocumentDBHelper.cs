using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interface
{
    public interface IDocumentDBHelper
    {
        List<dynamic> GetDocumnet(string DataBaseName, string CollectionName, string query, SqlParameterCollection sqlParameters, string partitionKey);
        List<dynamic> GetDocumnet(string DataBaseName, string CollectionName, string query, string partitionKey);
    }
}
