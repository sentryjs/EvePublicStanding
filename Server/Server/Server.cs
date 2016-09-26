using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

using Newtonsoft.Json;

using StackExchange.Redis;

using WebSocketSharp;
using WebSocketSharp.Server;
using eZet.EveLib.EveXmlModule;
using eZet.EveLib.EveXmlModule.Models;
using eZet.EveLib.EveXmlModule.Models.Character;

namespace EvePublicStanding
{
    public class EPS : WebSocketBehavior
    {
        public static ConnectionMultiplexer redisConn = ConnectionMultiplexer.Connect("localhost");
        //public WebSocketContext Context { get; set; }

        public static void Main(string[] args)
        {
            //protected WebSocketContext clientData = WebSocketContext();
            
            //var wssv = new WebSocketServer("ws://localhost:8080");
            var wssv = new WebSocketServer(8181);
            wssv.AddWebSocketService<EPS>("/",
                () => new EPS()
                {
                    IgnoreExtensions = true,
                    Protocol = "wamp"
                });
            wssv.Start();
            if (wssv.IsListening)
            {
                Console.WriteLine("Serving WebSocket on port {0}.", wssv.Port);
            }
            Console.ReadKey(true);
            wssv.Stop();
            //httpsv.Stop();
        }

        static string CleanInput(string strIn, int input)
        {
            if (input == 0)
            {
                // Replace invalid characters with empty strings.
                try
                {
                    return Regex.Replace(strIn, @"[!@#%&;<>?,./=\(\)\[\]\\\+\*\?\^\$]", "",
                                         RegexOptions.None, TimeSpan.FromSeconds(1.5));
                }
                // If we timeout when replacing invalid characters, 
                // we should return Empty.
                catch (RegexMatchTimeoutException)
                {
                    return String.Empty;
                }
            }
            else if (input == 1)
            {
                // Replace invalid characters with empty strings.
                try
                {
                    return Regex.Replace(strIn, @"[^a-zA-Z\d]", "",
                                         RegexOptions.None, TimeSpan.FromSeconds(1.5));
                }
                // If we timeout when replacing invalid characters, 
                // we should return Empty.
                catch (RegexMatchTimeoutException)
                {
                    return String.Empty;
                }
            }
            else if (input == 2)
            {
                // Replace invalid characters with empty strings.
                try
                {
                    return Regex.Replace(strIn, @"[^\w\d\s\.\-\']", "",
                                         RegexOptions.None, TimeSpan.FromSeconds(1.5));
                }
                // If we timeout when replacing invalid characters, 
                // we should return Empty.
                catch (RegexMatchTimeoutException)
                {
                    return String.Empty;
                }
            }
            return String.Empty;
        }

        protected override void OnOpen()
        {
            Console.WriteLine("New Connection: " + Context.UserEndPoint.Address + " - Request: " + Context.RequestUri.AbsolutePath);
            if (Context.RequestUri.AbsolutePath != "/")
            {
                Context.WebSocket.Close();
            }

        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Clean Disconnect: " + e.WasClean+ " / Reason: " + e.Reason);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception + ": " + e.Message);
        }


        protected override void OnMessage(MessageEventArgs e)
        {
            //var sanitized = CleanInput(e.Data, 0);
            Dictionary<string, string> query = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.Data);
            IDatabase redDb1 = redisConn.GetDatabase(10);
            IDatabase redDb2 = redisConn.GetDatabase(11);
            var jss = new JavaScriptSerializer();
            if (query.ContainsKey("term")) // Name search
            {
                //Console.WriteLine("Debug: Name Search");
                string sName = query["term"];
                string standing = redDb1.StringGet(sName);
                //////GETS STANDING//////
                if (!standing.IsNullOrEmpty())
                {
                    var dict = jss.Deserialize<Dictionary<string, string>>(standing);
                    var dStanding = dict["standing"];
                    var dCounter = dict["counter"];
                    //Console.WriteLine("Debug: "+ standing);
                    if (string.IsNullOrEmpty(dStanding))
                    {
                        Console.WriteLine("Search / Name: " + sName + " - Standing: Unknown");
                    }
                    else
                    {
                        Console.WriteLine("Search / Name: " + sName + " - Standing: " + (Convert.ToDouble(dStanding)/ Convert.ToDouble(dCounter)).ToString());
                    }
                    if (string.IsNullOrEmpty(dStanding))
                    {
                        //Console.WriteLine("Debug: SEND");
                        standing = "Unknown";
                        Dictionary<string, string> keyPair = new Dictionary<string, string>
                        {
                            { "Name",sName },
                            { "Standing",standing }
                        };
                        string json = JsonConvert.SerializeObject(keyPair, Formatting.Indented);
                        Send(json);
                    }
                    else
                    {
                        //Console.WriteLine("Debug: SEND");
                        Dictionary<string, string> keyPair = new Dictionary<string, string>
                        {
                            { "Name",sName },
                            { "Standing", (Convert.ToDouble(dStanding)/ Convert.ToDouble(dCounter)).ToString() }
                        };
                        string json = JsonConvert.SerializeObject(keyPair, Formatting.Indented);
                        Send(json);
                    }
                }
                else
                {
                    Console.WriteLine("Search / Name: " + sName + " - Standing: Unknown");
                    standing = "Unknown";
                    Dictionary<string, string> keyPair = new Dictionary<string, string>
                        {
                            { "Name",sName },
                            { "Standing",standing }
                        };
                    string json = JsonConvert.SerializeObject(keyPair, Formatting.Indented);
                    Send(json);
                }
            }
            else // Login
            {
                string sDID = query["did"].ToString();
                Console.WriteLine();
                int nextWeek = DateTime.Now.AddDays((int)DateTime.Now.DayOfWeek + 7).DayOfYear;
                if (!string.IsNullOrEmpty(redDb2.StringGet(sDID)))
                {
                    if (DateTime.Now.DayOfYear < int.Parse(redDb2.StringGet(sDID)))
                    {
                        //Console.WriteLine("2");
                        Console.WriteLine("API Abuse triggered");
                        Dictionary<string, string> keyPair = new Dictionary<string, string>
                    {
                        { "Name","Error: " },
                        { "Standing","Users may only submit one character per week." }
                    };
                        string json = JsonConvert.SerializeObject(keyPair, Formatting.Indented);
                        Send(json);
                        return;
                    }
                }
                else
                {
                    redDb2.StringSet(sDID, nextWeek);
                }
                //Console.WriteLine("5");
                //Console.WriteLine("Login");
                int keyID = int.Parse(CleanInput(query["keyid"], 1));
                string vCode = CleanInput(query["vcode"], 1);
                string cName = CleanInput(query["cname"], 2);

                Console.WriteLine("API Submitted / KeyID: " + sDID + " - vCode: " + vCode + " - Name: " + cName);

                var key = new ApiKey(keyID, vCode);
                if (key.KeyType == ApiKeyType.Character)
                {
                    //Console.WriteLine("Debug: 6");
                    CharacterKey cKey = (CharacterKey)key.GetActualKey();
                    Character character = cKey.Characters.Single(c => c.CharacterName == cName);

                    EveXmlResponse<ContactList> data = character.GetContactList();
                    int nContacts = data.Result.PersonalContacts.Count();
                    //Console.WriteLine("Debug: Count " + nContacts);
                    if (string.IsNullOrEmpty(redDb2.StringGet(sDID)))
                    //First submission
                    {
                        for (int i = 0; i < nContacts; i++)
                        {
                            string charName = redDb1.StringGet(data.Result.PersonalContacts[i].ContactName);
                            //Console.WriteLine("Debug: 8");
                            if (string.IsNullOrEmpty(charName))
                            // Contact new
                            {
                                //Console.WriteLine("Debug: 9");
                                //redDb1.StringSet(data.Result.PersonalContacts[i].ContactName, data.Result.PersonalContacts[i].Standing);
                                redDb1.StringSet(data.Result.PersonalContacts[i].ContactName, "{'standing':" + data.Result.PersonalContacts[i].Standing + ",'counter':1}");
                                //redDb2.StringSet(CleanInput(query["keyid"], 1), CleanInput(query["vcode"], 1));
                                Console.WriteLine("New Contact: " + data.Result.PersonalContacts[i].ContactName + " / {'standing':" + data.Result.PersonalContacts[i].Standing + ",'counter':1}");
                            }
                            else
                            // Contact exists
                            {
                                //Console.WriteLine("Debug: 10");
                                Dictionary<string, double> result = JsonConvert.DeserializeObject<Dictionary<string, double>>(charName);
                                double standing = result["standing"];
                                double counter = result["counter"];
                                redDb1.StringSet(data.Result.PersonalContacts[i].ContactName, "{'standing':" + ((standing + data.Result.PersonalContacts[i].Standing) / counter).ToString() + ",'counter':" + (counter+1) + "}".ToString());
                                //redDb2.StringSet(CleanInput(query["keyid"], 1), CleanInput(query["vcode"], 1));
                                Console.WriteLine("Update Contact: " + data.Result.PersonalContacts[i].ContactName + " / {'standing':" + ((standing + data.Result.PersonalContacts[i].Standing) / counter) + ",'counter':" + (counter+1) + "}");
                            }
                        }
                    }
                    else
                    // Repeat submission
                    {
                        for (int i = 1; i < nContacts; i++)
                        {
                            //Console.WriteLine("Debug: 11");
                            if (string.IsNullOrEmpty(redDb1.StringGet(data.Result.PersonalContacts[i].ContactName)))
                            // Contact new
                            {
                                //Console.WriteLine("Debug: 12");
                                //redDb1.StringSet(data.Result.PersonalContacts[i].ContactName, data.Result.PersonalContacts[i].Standing);
                                redDb1.StringSet(data.Result.PersonalContacts[i].ContactName, "{'standing':" + data.Result.PersonalContacts[i].Standing + ",'counter':1}".ToString());
                                //redDb2.StringSet(CleanInput(query["keyid"], 1), CleanInput(query["vcode"], 1));
                                Console.WriteLine("New Contact: " + data.Result.PersonalContacts[i].ContactName + " / {'standing':" + data.Result.PersonalContacts[i].Standing + ",'counter':1}");
                            }
                        }
                    }
                }
                else
                {
                    // Error: Not character
                    Console.WriteLine("Debug: -1");
                    Dictionary<string, string> keyPair = new Dictionary<string, string>
                    {
                        { "Name","Error: " },
                        { "Standing","Invalid API! Your API should be for a SINGLE character with a 16 bit Access Mask." }
                    };
                    string json = JsonConvert.SerializeObject(keyPair, Formatting.Indented);
                    Send(json);
                }
                redDb2.StringSet(sDID, nextWeek);
            }
            
        }
    }
}