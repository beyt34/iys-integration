using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Iys.Integration.Console.Extensions;
using Iys.Integration.Console.Model;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace Iys.Integration.Console {
    public class Program {
        private static string token;

        private static string baseUrl;

        private static string brandCode;

        private static string username;

        private static string password;

        private static string logFile;

        public static async Task Main(string[] args) {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            baseUrl = configuration.GetValue<string>("IysConfig:BaseUrl");
            brandCode = configuration.GetValue<string>("IysConfig:BrandCode");
            username = configuration.GetValue<string>("IysConfig:Username");
            password = configuration.GetValue<string>("IysConfig:Password");
            logFile = configuration.GetValue<string>("IysConfig:LogFile");

            await ProcessSingle();

            await ProcessMultiple();

            System.Console.WriteLine("\nPress Enter to exit...");
            System.Console.ReadLine();
        }

        private static async Task ProcessSingle() {
            // GetToken
            await GetToken();

            // get data
            var list = GetData(30);
            const int Processed = 0;
            for (var i = Processed; i < list.Count; i++) {
                var item = list[i];
                var consentRequestModel = new ConsentRequestModel();
                var valid = false;

                if (item.Type == "EPOSTA") {
                    if (EmailIsValid(item.Recipient)) {
                        consentRequestModel = new ConsentRequestModel(item.Type, item.Recipient, item.CreatedDateTime);
                        valid = true;
                    }
                } else {
                    if (item.Recipient.Length == 10) {
                        consentRequestModel = new ConsentRequestModel(item.Type, item.Recipient, item.CreatedDateTime);
                        valid = true;
                    }
                }

                if (valid) {
                    // add log
                    var msg = $"{DateTime.UtcNow} row:{i + 1:0#####}, consent:{consentRequestModel.SerializeToJson()}";
                    System.Console.WriteLine(msg);
                    AddLog(msg);

                    await AddConsent(consentRequestModel);
                    System.Threading.Thread.Sleep(100);
                }

                // get token every 1000 record
                if ((i + 1) % 1000 == 0) {
                    await GetToken();
                }
            }
        }

        private static async Task ProcessMultiple() {
            // GetToken
            await GetToken();

            // get data
            var list = GetData(3000);
            var requestList = new List<ConsentRequestModel>();
            const int Processed = 0;
            for (var i = Processed; i < list.Count; i++) {
                var item = list[i];
                var consentRequestModel = new ConsentRequestModel();
                var valid = false;

                if (item.Type == "EPOSTA") {
                    if (EmailIsValid(item.Recipient)) {
                        consentRequestModel = new ConsentRequestModel(item.Type, item.Recipient, item.CreatedDateTime);
                        valid = true;
                    }
                } else {
                    if (item.Recipient.Length == 10) {
                        consentRequestModel = new ConsentRequestModel(item.Type, item.Recipient, item.CreatedDateTime);
                        valid = true;
                    }
                }

                if (valid) {
                    // add log
                    var msg = $"{DateTime.UtcNow} row:{i + 1:0#####}, consent:{consentRequestModel.SerializeToJson()}";
                    System.Console.WriteLine(msg);
                    AddLog(msg);

                    requestList.Add(consentRequestModel);
                }

                // call api every 100 records
                if ((i + 1) % 100 == 0) {
                    await AddConsentMultiple(requestList);
                    requestList = new List<ConsentRequestModel>();
                }

                // get token every 10000 records
                if ((i + 1) % 10000 == 0) {
                    await GetToken();
                }
            }

            // process remain data
            await AddConsentMultiple(requestList);
        }

        private static async Task GetToken() {
            try {
                var url = baseUrl + "oauth2/token";
                var requestModel = new TokenRequestModel {
                    Username = username,
                    Password = password,
                    Type = "password",
                };

                using (var client = new HttpClient()) {
                    // post async given url
                    var content = JsonConvert.SerializeObject(requestModel);
                    var responseMessage = await client.PostAsync(url, new StringContent(content, Encoding.Default, "application/json"));
                    var response = await responseMessage.Content.ReadAsStringAsync();

                    var responseModel = JsonConvert.DeserializeObject<TokenResponseModel>(response);
                    token = responseModel.Token;

                    System.Console.WriteLine("Token:{0}\n", token);
                }
            } catch (Exception e) {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private static async Task AddConsent(ConsentRequestModel requestModel) {
            try {
                var url = baseUrl + $"sps/{brandCode}/brands/{brandCode}/consents";
                using (var client = new HttpClient()) {
                    // Request headers.
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    // post async given url
                    var content = JsonConvert.SerializeObject(requestModel);
                    var responseMessage = await client.PostAsync(url, new StringContent(content, Encoding.Default, "application/json"));
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    System.Console.WriteLine("{0}, consent-response:{1}", DateTime.UtcNow, response);
                    AddLog(response);

                    ////var responseModel = JsonConvert.DeserializeObject<ConsentResponseModel>(response);
                    ////System.Console.WriteLine("{0}, consent:{1}", DateTime.UtcNow, responseModel.SerializeToJson());
                }
            } catch (Exception e) {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private static async Task AddConsentMultiple(List<ConsentRequestModel> requestModel) {
            try {
                var url = baseUrl + $"sps/{brandCode}/brands/{brandCode}/consents/request";
                using (var client = new HttpClient()) {
                    // Request headers.
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    // post async given url
                    var content = JsonConvert.SerializeObject(requestModel);
                    var responseMessage = await client.PostAsync(url, new StringContent(content, Encoding.Default, "application/json"));
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    System.Console.WriteLine("\n{0}, consent-response:{1}\n", DateTime.UtcNow, response);
                    AddLog(response);

                    ////var responseModel = JsonConvert.DeserializeObject<ConsentResponseModel>(response);
                    ////System.Console.WriteLine("{0}, consent:{1}", DateTime.UtcNow, responseModel.SerializeToJson());
                }
            } catch (Exception e) {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private static void AddLog(string logText) {
            var sb = new StringBuilder();
            var msg = $"\n{DateTime.UtcNow} Log:{logText}";

            sb.Append(msg);

            File.AppendAllText(logFile, sb.ToString());
            sb.Clear();
        }

        private static bool EmailIsValid(string emailaddress) {
            try {
                var m = new MailAddress(emailaddress);
                ////var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                ////var match = regex.Match(emailaddress);
                ////return match.Success;

                return Regex.IsMatch(emailaddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            } catch (FormatException) {
                return false;
            }
        }

        private static List<CustomerModel> GetData(int total) {
            // create sample data
            var per = total / 3;
            var list = new List<CustomerModel>();
            for (var i = 0; i < per; i++) {
                var customer = new CustomerModel {
                    Id = i,
                    Type = "EPOSTA",
                    Recipient = Guid.NewGuid().ToString() + "@company.com",
                    CreatedDateTime = DateTime.UtcNow
                };
                list.Add(customer);
            }

            for (var i = per; i < per * 2; i++) {
                var tick = DateTime.UtcNow.Ticks.ToString();
                var customer = new CustomerModel {
                    Id = i,
                    Type = "MESAJ",
                    Recipient = "535" + tick.Substring(tick.Length - 7),
                    CreatedDateTime = DateTime.UtcNow
                };
                list.Add(customer);
                System.Threading.Thread.Sleep(1);
            }

            for (var i = per * 2; i < total; i++) {
                var tick = DateTime.UtcNow.Ticks.ToString();
                var customer = new CustomerModel {
                    Id = i,
                    Type = "ARAMA",
                    Recipient = "545" + tick.Substring(tick.Length - 7),
                    CreatedDateTime = DateTime.UtcNow
                };
                list.Add(customer);
                System.Threading.Thread.Sleep(1);
            }

            return list;
        }
    }
}
