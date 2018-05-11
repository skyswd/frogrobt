using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Configuration;
using System.Web;
using System.IO;
using System.Collections.Generic;
using HtmlAgilityPack;
using Microsoft.Bot.Builder.Dialogs;

namespace frogRobot
{
    [BotAuthentication]
    //[BotAuthentication(MicrosoftAppId = "47713bd9-109c-4f32-8df2-9f94292aa6b2", MicrosoftAppPassword = "mtwHUPNVV34[*^xgmaI986*")]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // Activity 是Bot Framework 3.0的寫法，若是改版後這種寫法無法使用
            // 請在文章下方留言告訴我，以進行內容的更新與修改
            activity.Text = activity.Text.Trim().Replace("@frogRobotV1 ", "").Replace("frogRobotV1 ", "");
            string LUISMessage = activity.Text.Trim();//要傳給LUIS的字串
            string LUISHEADDATA = "";//記錄有解析出的Head標籤
            bool SMeOnly = false;//強制設定為小Me 的查詢

            bool BeforeSmallMeChk = false;



            if (activity != null && activity.Type == ActivityTypes.Message)
            {
                //原始程式
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                await connector.Conversations.ReplyToActivityAsync(reply);

                //ConnectorClient connector;
                //string strReply;
                ////呼叫Luis設定，並決定回傳訊息
                //ChkLuis(activity, LUISMessage, LUISHEADDATA, SMeOnly, BeforeSmallMeChk, out connector, out strReply);

                //Activity reply = activity.CreateReply(strReply);
                //await connector.Conversations.ReplyToActivityAsync(reply);

            }
            else
            {
                HandleSystemMessage(activity);
            }

            ////回覆資料時順便回覆一份給自己
            //if (activity.From.Id != ConfigurationManager.AppSettings["MytalkId"].ToString())
            //{
            //    GiveAnoterSkypeTalk(activity);
            //}
            //else
            //{
            //    GiveAnoterSkypeTalk_OnlySend(activity);
            //}
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }


        /// <summary>
        /// Luis設定
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="LUISMessage"></param>
        /// <param name="LUISHEADDATA"></param>
        /// <param name="SMeOnly"></param>
        /// <param name="BeforeSmallMeChk"></param>
        /// <param name="connector"></param>
        /// <param name="strReply"></param>
        private void ChkLuis(Activity activity, string LUISMessage, string LUISHEADDATA, bool SMeOnly, bool BeforeSmallMeChk, out ConnectorClient connector, out string strReply)
        {
            #region LUIS 設定
            //抓取LUIS 的方法一.
            //設定LUIS 的KEY
            string strLuisKey = ConfigurationManager.AppSettings["LUISAPIKey"].ToString();
            //設定LUIS 的AppId
            string strLuisAppId = ConfigurationManager.AppSettings["LUISAppId"].ToString();
            //設定本次要傳入的訊息
            string strMessage = HttpUtility.UrlEncode(LUISMessage);
            //Luis連線位址設定
            string strLuisUrl = $"https://api.projectoxford.ai/luis/v1/application?id={strLuisAppId}&subscription-key={strLuisKey}&q={strMessage}";

            connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            activity.Recipient.Name = "年輕有為的小探蛙";

            //// 收到文字訊息後，往LUIS送
            WebRequest request = WebRequest.Create(strLuisUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string json = reader.ReadToEnd();
            frogRobot.LUISResult objLUISRes = JsonConvert.DeserializeObject<frogRobot.LUISResult>(json);
            strReply = "無法識別的內容V";
            #endregion

            if (activity.Text.Trim() == "Aurora三點到了")
            {
                GiveAnoterSkypeTalk_OnlySendthree();//系統三部的群組
                GiveAnoterSkypeTalk_OnlySendthree_test();//小探蛙敢死隊群組
            }
            else if (activity.Text.Trim().StartsWith("主人輸出:") == true)
            {
                GiveAnoterSkypeTalk_OnlySend_onlinetype(activity.Text.Trim().Replace("主人輸出:", ""));
            }

            else if (objLUISRes.intents.Count > 0)
            {
                string strIntent = objLUISRes.intents[0].intent;
                string strQuest = "";
                //else if(activity.Text.Trim()== "Aurora三點到了")
                //{
                //    GiveAnoterSkypeTalk_OnlySendthree();//系統三部的群組
                //    GiveAnoterSkypeTalk_OnlySendthree_test();//小探蛙敢死隊群組
                //}
                //else if(activity.Text.Trim().StartsWith("主人輸出:")==true)
                //{
                //    GiveAnoterSkypeTalk_OnlySend_onlinetype(activity.Text.Trim().Replace("主人輸出:", ""));
                //}
                    #region LUIS 分類
                    switch (strIntent)
                    {
                        case "問題":
                            strQuest = (objLUISRes.entities.Find((x => x.type == "詢問")).entity == null ? "詢問" : objLUISRes.entities.Find((x => x.type == "詢問")).entity);
                            string strDate = (objLUISRes.entities.Find((x => x.type == "時間")).entity == null ? "" : objLUISRes.entities.Find((x => x.type == "時間")).entity);
                            string strPrg = (objLUISRes.entities.Find((x => x.type == "程式名稱/代號")).entity == null ? "" : objLUISRes.entities.Find((x => x.type == "程式名稱/代號")).entity);
                            strReply = $"您要詢問的程式名稱/代號:{strPrg}，日期:{strDate}，類型是:{strQuest}。我馬上幫您找出資訊";
                            strReply += "此問題會納入下次修改的指標，後續會做修正~";
                            break;
                        case "問題1":
                            strQuest = (objLUISRes.entities.Find((x => x.type == "詢問")).entity == null ? "詢問" : objLUISRes.entities.Find((x => x.type == "詢問")).entity);
                            strReply = $"您要詢問的問題類型是:{strQuest}。我馬上幫您找出資訊";
                            strReply += "此問題會納入下次修改的指標，後續會做修正~多謝提報!!";
                            break;
                        case "建議":
                            strReply = $"此建議已收錄，感謝建議~~";
                            break;
                        case "只是打招呼":
                            strReply = "您好，有什麼能幫得上忙的呢?";
                            break;
                        case "地址":
                            strReply = $"您要切分的地址為::{LUISMessage}。我馬上幫您切分地址資訊:\n\n"; ;
                            //strReply += ADDRESS(LUISMessage);
                            break;
                        case "None":
                            if (activity.Text == "主人要專用資料")
                            {
                                strReply = GetCHANNELDATA(activity);
                            }
                            else if (activity.Text == "主人測試資料")
                            {
                                GiveAnoterSkypeTalk(activity);
                                strReply = "test";
                            }
                            else
                            {
                                //strReply = $"我是黃巾小探馬，目前沒有此問題的相關訊息，在此提供網上的搜詢訊息提供給看倌參考:\t\n HTTPS://WWW.GOOGLE.COM.TW/search?q=" + strMessage + "&oq=" + strMessage;
                                strReply = $"我是小探蛙，目前沒有此問題的相關訊息，在此提供網上的搜詢訊息提供給看倌參考:\t\n HTTPS://WWW.GOOGLE.COM.TW/search?q=" + strMessage + "&oq=" + strMessage;
                            }
                            break;
                        case "問黃巾動向":
                        case "黃巾":
                        case "黃巾賊":
                        case "賊寇":
                            strReply = ClawInfoFromWeb();
                            break;
                        default:
                            strReply = @"您在說什麼，我聽不懂~~~(/.\)";
                            break;
                    }
                    #endregion

            }
            else
            {
                strReply = "您在說什麼，我聽不懂~~~(@.@)";
            }
        }

        #region 黃巾相關
        private string ClawInfoFromWeb()
        {
            var result = "";
            // 1.抓當月清單
            var fightSpace = ClawItccWebSite(DateTime.Now.ToString("yyyy"), DateTime.Now.AddMonths(0).ToString("MM"));

            //2017/12/11 加上跨月時的判斷 
            //==================================================
            //取出下月日期
            var nextMonthDate = System.DateTime.Now.AddMonths(1).ToString("yyyy/MM") + "/01";

            var nextyear = DateTime.Parse(nextMonthDate).ToString("yyyy");
            var nextday = DateTime.Parse(nextMonthDate).ToString("MM");
            //==================================================

            // 2.抓次月清單 & 併入
            //fightSpace.AddRange(ClawItccWebSite(DateTime.Now.ToString("yyyy"), DateTime.Now.AddMonths(1).ToString("MM")));
            fightSpace.AddRange(ClawItccWebSite(nextyear, nextday));

            //2018/05/01 增加查無資料的判斷
            if (fightSpace.Count() == 0)
            {
                return "查無資料";
            }

            // 3.篩選搶車位的黃領帶
            var MotelCycle = new MotelCycle();
            var MotelCyclea = fightSpace
                .Where(x => x.topic.Contains("永慶")).ToList();
            //.ForEach(x => Console.WriteLine(DateTime.Now.ToString("yyyy") + " / " + x.month + " / " + x.day + "\t" + x.topic + "\t" + x.room));

            result = string.Join("", MotelCyclea.Select(x => x.year + " / " + x.month + " / " + x.day + "\t" + x.topic + "\t" + x.room + "\t\n").ToArray());
            return result;
        }

        private List<TheMeeting> ClawItccWebSite(string searchYear, string searchMonth)
        {
            var result = new List<TheMeeting>();

            // 1.使用 HtmlAgilityPack 分析 XPath
            HtmlWeb webClient = new HtmlWeb();

            // 2.將網址放入在webClient.Load
            HtmlDocument doc = webClient.Load($"http://www.ticc.com.tw/main_ch/EventsCalendar.aspx?uid=146&pid=&YYYY={searchYear}&MM={searchMonth}&DD=01#");

            // 3.取得要分析的 HTML 節點 (div list)
            HtmlNodeCollection divList = doc.DocumentNode.SelectNodes(@"/html/body/div[3]/div/div/div[3]/div");

            foreach (HtmlNode dailyMeetings in divList)
            {
                // 4.div class=list 才有每日的會議清單
                if (!dailyMeetings.GetAttributeValue("class", "").Contains("list"))
                    continue;

                var year = searchYear;
                var month = dailyMeetings.SelectNodes("./div[1]/div[1]")[0].InnerText;
                var day = dailyMeetings.SelectNodes("./div[1]/div[2]")[0].InnerText;

                var customList = dailyMeetings.SelectNodes("./div[2]/div");

                foreach (HtmlNode custom in customList)
                {
                    var topic = custom.SelectNodes("./div[1]")[0].InnerText;
                    var room = custom.SelectNodes("./div[2]")[0].InnerText;

                    result.Add(new TheMeeting() { year = searchYear, month = month, day = day, topic = topic, room = room });
                }
            }

            return result;
        }

        private class TheMeeting
        {
            public string year { get; set; }
            public string month { get; set; }

            public string day { get; set; }

            public string topic { get; set; }

            public string room { get; set; }
        }

        private class MotelCycle
        {
            public string date { get; set; }

            public string topic { get; set; }

            public string room { get; set; }
        }
        #endregion

        #region 取得Skype bot 頻道相關的傳輸資料
        /// <summary>
        /// 型態:
        ///  FromId            ->   ChannelAccount
        ///  RecipientId       ->   ChannelAccount
        ///  ConversationId    ->   ConversationAccount
        ///  
        /// ChannelAccount 物件內容如下：
        /// 變數名稱	型態
        ///   Id        string
        ///  Name       string
        /// 
        /// ConversationAccount 物件內容如下：
        ///變數名稱     型態
        ///   Id        string
        ///  Name       string
        /// IsGroup     bool?
        /// </summary>

        private class CHANNLDATA
        {
            public ChannelAccountData From { get; set; }//傳送者 (從誰傳出)
            public ChannelAccountData Recipient { get; set; }//接受者 (誰接受資訊)	
            public ConversationAccountData Conversation { get; set; }//(哪一個對話)
        }
        private class ChannelAccountData
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class ConversationAccountData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool? IsGroup { get; set; }
        }
        /// <summary>
        /// 取得skypebot頻道的位置資料
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private string GetCHANNELDATA(Activity activity)
        {
            var ChannelData = new CHANNLDATA();

            var FormData = new ChannelAccountData();
            var RecipientData = new ChannelAccountData();
            var ConversationData = new ConversationAccountData();

            FormData.Id = activity.From.Id;
            FormData.Name = activity.From.Name;

            RecipientData.Id = activity.Recipient.Id;
            RecipientData.Name = activity.Recipient.Name;

            ConversationData.Id = activity.Conversation.Id;
            ConversationData.Name = activity.Conversation.Name;
            ConversationData.IsGroup = activity.Conversation.IsGroup;

            ChannelData.From = FormData;
            ChannelData.Recipient = RecipientData;
            ChannelData.Conversation = ConversationData;

            return JsonConvert.SerializeObject(ChannelData);
        }
        #endregion

        #region 測試獨立回傳
        /// <summary>
        /// 有別人呼叫小探蛙時回傳ID 及呼叫的內容到自己的skype上
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GiveAnoterSkypeTalk(Activity activity)
        {
            //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
            var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
            IMessageActivity newMessage = Activity.CreateMessageActivity();
            newMessage.Type = ActivityTypes.Message;
            newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
            newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
            newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
            newMessage.Text = $"From-ID:" + activity.From.Id +
                               " --> From-Name:" + activity.From.Name +
                               " --> ID:" + activity.Id +
                               " --> ChannelId:" + activity.ChannelId +
                               " --> Conversation-id:" + activity.Conversation.Id +
                               " --> Conversation-IsGroup:" + activity.Conversation.IsGroup +
                               " --> 詢問的問題:" + activity.Text.Trim();
            await connector.Conversations.SendToConversationAsync((Activity)newMessage);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        /// <summary>
        /// 測試回拋群組3點到了的訊息Api 拋給系統三部的群組
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GiveAnoterSkypeTalk_OnlySendthree()
        {
            try
            {
                Activity activity = new Activity();
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.ChannelId = "skype";


                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                //小探蛙敢死隊的群組
                //==========================
                //newMessage.Id = "1525671952797";
                //newMessage.Conversation = new ConversationAccount(true, "19:e77c86cbbc64404da9f8a9666cdea826@thread.skype", null);
                //==========================

                //系統三部的skype群組
                //=================================
                newMessage.Id = "1525673070949";
                newMessage.Conversation = new ConversationAccount(true, "19:I3NreXNreXN3ZDEvJDEzNmY1NDUzN2UxY2EyNA==@p2p.thread.skype", null);
                //=================================
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
                newMessage.Text = $"三點到了，該起來動一動了!!!";
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception E)
            {
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());

                newMessage.Text = "ERRPR:" + E.Message;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }

        /// <summary>
        /// 測試回拋群組3點到了的訊息Api 拋給小探蛙敢死隊的群組
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GiveAnoterSkypeTalk_OnlySendthree_test()
        {
            try
            {
                Activity activity = new Activity();
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.ChannelId = "skype";


                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                //小探蛙敢死隊的群組
                //==========================
                newMessage.Id = "1525671952797";
                newMessage.Conversation = new ConversationAccount(true, "19:e77c86cbbc64404da9f8a9666cdea826@thread.skype", null);
                //==========================

                //系統三部的skype群組
                //=================================
                //newMessage.Id = "1525673070949";
                //newMessage.Conversation = new ConversationAccount(true, "19:I3NreXNreXN3ZDEvJDEzNmY1NDUzN2UxY2EyNA==@p2p.thread.skype", null);
                //=================================
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
                newMessage.Text = $"三點到了，便當該帶頭，起來動一動了!!!";
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception E)
            {
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());

                newMessage.Text = "ERRPR:" + E.Message;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }

        /// <summary>
        /// 取得群組數值的
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GiveAnoterSkypeTalk_OnlySend(Activity activity)
        {
            try
            {
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.ChannelId = "skype";
                newMessage.Id = "1525660436131";
                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
                newMessage.Text = $"FromID:" + activity.From.Id +
                                   " --> FromName:" + activity.From.Name +
                                   " --> ChannelId:" + activity.ChannelId +
                                   " --> Id:" + activity.Id +
                                   " --> ServiceUrl:" + activity.ServiceUrl +
                                   " --> Summary:" + activity.Summary +
                                   " --> Text:" + activity.Text +
                                   " --> TopicName:" + activity.TopicName +
                                   " --> Action:" + activity.Action +
                                   " --> AttachmentLayout:" + activity.AttachmentLayout +
                                   " --> Conversation.Id:" + activity.Conversation.Id +
                                   " --> Conversation.IsGroup:" + activity.Conversation.IsGroup +
                                   " --> Conversation.Name:" + activity.Conversation.Name +
                                   " --> HistoryDisclosed:" + activity.HistoryDisclosed +
                                   " --> ReplyToId:" + activity.ReplyToId +
                                   " --> Properties:" + activity.Properties +
                                   " --> TextFormat:" + activity.TextFormat +
                                   " --> Timestamp:" + activity.Timestamp +
                                   " --> Type:" + activity.Type +
                                   " --> Recipient.Id:" + activity.Recipient.Id +
                                   " --> Recipient.Name:" + activity.Recipient.Name +
                                   " --> 詢問的問題:" + activity.Text.Trim();
                //newMessage.Text = $"三點到了!!!";
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception E)
            {
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
                newMessage.Text = "ERRPR:" + E.Message;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }

        /// <summary>
        /// 訊息Api 拋給小探蛙敢死隊的群組
        /// 依照輸入的文字輸出
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GiveAnoterSkypeTalk_OnlySend_onlinetype(string inputmessage)
        {
            try
            {
                Activity activity = new Activity();
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.ChannelId = "skype";


                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                //小探蛙敢死隊的群組
                //==========================
                newMessage.Id = "1525671952797";
                newMessage.Conversation = new ConversationAccount(true, "19:e77c86cbbc64404da9f8a9666cdea826@thread.skype", null);
                //==========================

                //系統三部的skype群組
                //=================================
                //newMessage.Id = "1525673070949";
                //newMessage.Conversation = new ConversationAccount(true, "19:I3NreXNreXN3ZDEvJDEzNmY1NDUzN2UxY2EyNA==@p2p.thread.skype", null);
                //=================================
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());
                //newMessage.Text = $"三點到了，冠州該帶頭，起來動一動了!!!";
                newMessage.Text = inputmessage;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception E)
            {
                //MicrosoftAppCredentials.TrustServiceUrl("https://skype.botframework.com");
                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.From = new ChannelAccount(ConfigurationManager.AppSettings["MytalkId"].ToString(), "主人");
                newMessage.Conversation = new ConversationAccount(null, ConfigurationManager.AppSettings["MytalkId"].ToString(), null);
                newMessage.Recipient = new ChannelAccount(ConfigurationManager.AppSettings["MyBottalkId"].ToString(), ConfigurationManager.AppSettings["BotId"].ToString());

                newMessage.Text = "ERRPR:" + E.Message;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }
        #endregion


    }
}