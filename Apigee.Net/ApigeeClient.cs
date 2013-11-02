using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apigee.Net.Models.ApiResponse;
using Apigee.Net.Networking;
using Apigee.Net.Models;
using Newtonsoft.Json.Linq;

namespace Apigee.Net
{
    public class ApigeeClient
    {
        /// <summary>
        /// Create a new Apigee Client
        /// </summary>
        /// <param name="userGridUrl">The Base URL To the UserGrid</param>
        public ApigeeClient(string userGridUrl)
        {
            this.UserGridUrl = userGridUrl;
        }

        public string UserGridUrl { get; set; }

        #region Core Worker Methods

        /// <summary>
        /// Combines The UserGridUrl abd a provided path - checking to emsure proper http formatting
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string BuildPath(string path)
        {
            StringBuilder sbResult = new StringBuilder();
            sbResult.Append(this.UserGridUrl);
            
            if (this.UserGridUrl.EndsWith("/") != true)
            {
                sbResult.Append("/");
            }

            if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }

            sbResult.Append(path);

            return sbResult.ToString();
        }

        private JToken GetEntitiesFromJson(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson) != true)
            {
                var objResult = JObject.Parse(rawJson);
                return objResult.SelectToken("entities");
            }
            return null;
        }

        /// <summary>
        /// Performs a Get agianst the UserGridUrl + provided path
        /// </summary>
        /// <typeparam name="retrunT">Return Type</typeparam>
        /// <param name="path">Sub Path Of the Get Request</param>
        /// <returns>Object of Type T</returns>
        public retrunT PerformRequest<retrunT>(string path)
        {
            return PerformRequest<retrunT>(path, HttpTools.RequestTypes.Get, null);
        }

        /// <summary>
        /// Performs a Request agianst the UserGridUrl + provided path
        /// </summary>
        /// <typeparam name="retrunT">Return Type</typeparam>
        /// <param name="path">Sub Path Of the Get Request</param>
        /// <returns>Object of Type T</returns>
        public retrunT PerformRequest<retrunT>(string path, HttpTools.RequestTypes method, object data)
        {
            string requestPath = BuildPath(path);
            return HttpTools.PerformJsonRequest<retrunT>(requestPath, method, data);
        }

        

        #endregion

        #region Account Management
        private ApigeeUserModel SetUserInfo(JToken jsonData, string[] additionalValues = null)
        {

            IDictionary<string, string> extraEntities = new Dictionary<string, string>();
            if (additionalValues != null)
            {
                foreach (string customEntity in additionalValues)
                {
                    extraEntities.Add(customEntity, (jsonData[customEntity] ?? "").ToString());
                }
            }
        return new ApigeeUserModel
            {
                Uuid = (jsonData["uuid"] ?? "").ToString(),
                Username = (jsonData["username"] ?? "").ToString(),
                Password = (jsonData["password"] ?? "").ToString(),
                Lastname = (jsonData["lastname"] ?? "").ToString(),
                Firstname = (jsonData["firstname"] ?? "").ToString(),
                Title = (jsonData["title"] ?? "").ToString(),
                Email = (jsonData["Email"] ?? "").ToString(),
                Tel = (jsonData["tel"] ?? "").ToString(),
                HomePage = (jsonData["homepage"] ?? "").ToString(),
                Bday = (jsonData["bday"] ?? "").ToString(),
                Picture = (jsonData["picture"] ?? "").ToString(),
                Url = (jsonData["url"] ?? "").ToString(),
                CustomProperties = extraEntities

            };
            
        }
        //UUID can also be username apigee supports both after /users/
        public ApigeeUserModel GetUser(string uuid,string[] additionalValues=null)
        {
            var rawResults = PerformRequest<string>("/users/" + uuid);
            var jsonData = GetEntitiesFromJson(rawResults);
            if (jsonData.Count() > 1)
            {
                throw new Exception("Multiple Results Returned Where Only One Was Expected");
            }
            return SetUserInfo(jsonData[0], additionalValues);
            
        }
        public List<ApigeeUserModel> GetUsers()
        {
            var rawResults = PerformRequest<string>("/users");
            var users = GetEntitiesFromJson(rawResults);
            
            List<ApigeeUserModel> results = new List<ApigeeUserModel>();
            foreach (var usr in users)
            {
                results.Add(SetUserInfo(usr));
            }

            return results;
        }

        public string CreateAccount(ApigeeUserModel accountModel)
        {
            var rawResults = PerformRequest<string>("/users", HttpTools.RequestTypes.Post, accountModel);
            var entitiesResult = GetEntitiesFromJson(rawResults);
            if (entitiesResult != null)
            {
                return entitiesResult[0]["uuid"].ToString();
            }
            else
            {
                return UpdateAccount(accountModel);
            }
        }

        public string UpdateAccount(ApigeeUserModel accountModel)
        {
            var rawResults = PerformRequest<string>("/users/" + accountModel.Username, HttpTools.RequestTypes.Put, accountModel);

            return "";
        }

        #endregion

        #region Token Management

        public string GetToken(string username, string password)
        {
            var reqString = string.Format("/token/?grant_type=password&username={0}&password={1}", username, password);
            var rawResults = PerformRequest<string>(reqString);
            var results = JObject.Parse(rawResults);

            return results["access_token"].ToString();
        }

        public string LookUpToken(string token)
        {
            var reqString = "/users/me/?access_token=" + token;
            var rawResults = PerformRequest<string>(reqString);
            var entitiesResult = GetEntitiesFromJson(rawResults);

            return entitiesResult[0]["username"].ToString();
        }

        #endregion

    }
}
