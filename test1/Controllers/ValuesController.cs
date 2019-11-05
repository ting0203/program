using isRock.LineBot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace test1.Controllers
{
    public class ValuesController : ApiController
    {
        //要存到資料庫的資料
        public Dictionary<string, string> data = new Dictionary<string, string>();
        const string MyLineChannelAccessToken = "ilORU/C863975YgyLuXXBcQh/h2F5npSyKCHWti7f2ez2deO52njtVUJ9IYd6sdMShY3IeJA/raByZ75zIQHUyvfSQYtMSZ/5wuVjg75GFp2rOcKwD+93NSYdpigMTrbHAJIiPAl92poOXCxG5bs/gdB04t89/1O/w1cDnyilFU=";
        //建立Bot instance
        isRock.LineBot.Bot bot =
            new isRock.LineBot.Bot(MyLineChannelAccessToken);  //傳入Channel access token
        Event item;
        string postData;
        ReceievedMessage ReceivedMessage;

        /// <summary>
        /// Line機器人回覆API
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Post()
        {
            //string MyLineChannelAccessToken = "ilORU/C863975YgyLuXXBcQh/h2F5npSyKCHWti7f2ez2deO52njtVUJ9IYd6sdMShY3IeJA/raByZ75zIQHUyvfSQYtMSZ/5wuVjg75GFp2rOcKwD+93NSYdpigMTrbHAJIiPAl92poOXCxG5bs/gdB04t89/1O/w1cDnyilFU=";
            //建立Bot instance
            //isRock.LineBot.Bot bot =
            //    new isRock.LineBot.Bot(MyLineChannelAccessToken);  //傳入Channel access token
            string json = null;
            
            try
            {
                //取得 http Post RawData(should be JSON)
                postData = Request.Content.ReadAsStringAsync().Result;
                //剖析JSON
                ReceivedMessage = isRock.LineBot.Utility.Parsing(postData);
                //回覆訊息
                string Message = "";
                var item = ReceivedMessage.events.FirstOrDefault();

                //判斷是否為群組或個人
                string targetId = "";
                string usertId = "";
                if (ReceivedMessage.events[0].source.type == "group")
                {
                    //把Groupid加入資料庫的GroupInfo
                    targetId = ReceivedMessage.events[0].source.groupId;
                    usertId = ReceivedMessage.events[0].source.userId;
                }
                else if (ReceivedMessage.events[0].source.type == "room")
                {
                    //把Roomid加入資料庫的RoomInfo
                    targetId = ReceivedMessage.events[0].source.roomId;
                    usertId = ReceivedMessage.events[0].source.userId;
                }
                else
                {
                    //把userid加入資料庫的UserInfo
                    targetId = ReceivedMessage.events[0].source.userId;
                    usertId = ReceivedMessage.events[0].source.userId;
                }
                

                if (ReceivedMessage.events.FirstOrDefault().type == "follow")
                {
                    //新朋友來了(或解除封鎖)
                    var userInfo = bot.GetUserInfo(ReceivedMessage.events.FirstOrDefault().source.userId);
                    bot.ReplyMessage(ReceivedMessage.events.FirstOrDefault().replyToken, $"哈，'{userInfo.displayName}' 你來了...歡迎");
                    return Ok();
                }

                /*if (ReceivedMessage.events.FirstOrDefault().type == "join")
                {
                    isRock.LineBot.Utility.GetGroupMemberUserIDs();
                }*/

                //bot.PushMessage(targetId, postData);
                if (ReceivedMessage.events[0].type == "memberJoined")        //回傳訊息為message||下方有postdata
                {
                    LineUserInfo UserInfo = null;
                    if (item.source.type.ToLower() == "group"){
                        UserInfo = isRock.LineBot.Utility.GetGroupMemberProfile(
                            item.source.groupId, item.joined.members[0].userId, MyLineChannelAccessToken);
                    }
                    //顯示用戶名稱
                    if (item.source.type.ToLower() != "user"){
                        data.Add("groupid", item.source.groupId);
                        data.Add("userid", item.joined.members[0].userId);
                        data.Add("name", UserInfo.displayName);
                        json = JsonConvert.SerializeObject(data);
                        //Message += "\n你是:" + UserInfo.displayName;
                    }

                    bot.PushMessage(targetId, json);
                    GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_insertuser.php?data={json}");
                    //bot.ReplyMessage(ReceivedMessage.events.FirstOrDefault().replyToken, Message);
                    bot.PushMessage(targetId, $"哈 {UserInfo.displayName}...歡迎");
                }
                else if (ReceivedMessage.events[0].type == "message")        //回傳訊息為message||下方有postdata
                {
                    if (ReceivedMessage.events[0].message.type == "text")      //回傳訊息為文字||可為為貼圖...
                    {
                        string[] words = ReceivedMessage.events[0].message.text.Split(' ');
                        if(words[0] == "註冊")
                        {
                            data.Add("groupid", item.source.groupId);
                            data.Add("name", words[1]);
                            data.Add("othername", words[2]);
                            json = JsonConvert.SerializeObject(data);
                            string result = GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_registered.php?data={json}");
                            string jsonArrayText1 = result;
                            JArray ja = (JArray)JsonConvert.DeserializeObject(jsonArrayText1);
                            string ja1a = ja[0]["result"].ToString();
                            /*if (result == "success"){
                                bot.PushMessage(targetId, $"{words[2]}...已新增");
                            }
                            else if(result == "error"){
                                bot.PushMessage(targetId, $"{words[1]}錯誤");
                            }*/
                        }
                        else if (words[0] == "查")
                        {
                            data.Add("groupid", item.source.groupId);
                            data.Add("queryname", words[1]);
                            json = JsonConvert.SerializeObject(data);
                            string result = GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_queryname.php?data={json}");
                            string jsonArrayText1 = result;
                            JArray ja = (JArray)JsonConvert.DeserializeObject(jsonArrayText1);
                            string ja1a = ja[0]["result"].ToString();
                            //bot.PushMessage(targetId, ja1a);
                            if (ja[0]["result"].ToString() == "success"){
                                bot.PushMessage(targetId, ja[0]["data"]["userid"].ToString());
                            }
                            else if(ja[0]["result"].ToString() == "error"){
                                bot.PushMessage(targetId, ja[0]["data"].ToString());
                            }
                        }
                        else if(ReceivedMessage.events[0].message.text == "有誰")
                        {
                            data.Add("groupid", item.source.groupId);
                            json = JsonConvert.SerializeObject(data);
                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            string phone_url = "line://app/1651306906-NWLA8wl2?data=" + HttpUtility.UrlEncode(json);

                            actions.Add(new isRock.LineBot.UriAction() { label = "已註冊名單", uri = new Uri(phone_url) });
                            //單一Button Template Message
                            var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
                            {
                                altText = "替代文字(在無法顯示Button Template的時候顯示)",
                                text = "text",
                                title = "已為您查詢",
                                actions = actions //設定回覆動作
                            };
                            //發送
                            bot.PushMessage(targetId, ButtonTemplate);
                        }
                        else if (ReceivedMessage.events[0].message.text == "新增")     //訊息為新增時，判斷為個人聊天室或群組聊天室再分別將資料新增至資料庫
                        {
                            if (ReceivedMessage.events[0].source.type == "user")
                            {
                                data.Add("userid", targetId);
                                data.Add("thing", "糖果");
                                data.Add("place", "雜貨店");
                                data.Add("time", "下課後");
                                json = JsonConvert.SerializeObject(data);       //轉為json格式
                            }
                            if (ReceivedMessage.events[0].source.type == "group")
                            {
                                data.Add("groupid", targetId);
                                data.Add("people", "Berry");
                                data.Add("thing", "糖果");
                                data.Add("place", "雜貨店");
                                data.Add("time", "下課後");
                                json = JsonConvert.SerializeObject(data);                            
                            }
                            if (ReceivedMessage.events[0].source.type == "room")
                            {
                                data.Add("groupid", targetId);
                                data.Add("people", "Berry");
                                data.Add("thing", "糖果");
                                data.Add("place", "雜貨店");
                                data.Add("time", "下課後");
                                json = JsonConvert.SerializeObject(data);
                            }
                            bot.PushMessage(targetId, json);        //將得到json格式印出
                            bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_insertjson.php?data={json}"));       //用GET的方式得到json 進行API
                            bot.PushMessage(targetId, "done");
                        }
                        else if (words[0] == "查詢")
                        {
                            if (words[1] == "我"){   //此user的資料+群組內有此user資料
                                data.Add("userid", targetId);
                                json = JsonConvert.SerializeObject(data);
                                bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                                bot.PushMessage(targetId, "done");
                            }
                            if (words[1] == "群組")   //群組內的所有資料
                            {
                                data.Add("groupid", "Cc45bb5f1c13d089681d1a3332f93ed98");
                                json = JsonConvert.SerializeObject(data);
                                bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                                bot.PushMessage(targetId, "done");
                            }
                            if (words[1] == "全部")     //群組資料與個人資料
                            {
                                data.Add("userid", targetId);
                                data.Add("groupid", "Cc45bb5f1c13d089681d1a3332f93ed98");
                                json = JsonConvert.SerializeObject(data);
                                bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                                bot.PushMessage(targetId, "done");
                            }
                        }
                        else if (words[0] == "測試")
                        {
                            bot.PushMessage(targetId, "測試測試");
                        }
                        else if (ReceivedMessage.events[0].message.text == "ButtonTemplate")
                        {
                            string Flex = @"
                                    [
                                        {
                                            ""type"": ""template"",
                                            ""altText"": ""This is a buttons template"",
                                            ""template"": {
                                                ""type"": ""buttons"",
                                                ""imageAspectRatio"": ""rectangle"",
                                                ""imageSize"": ""cover"",
                                                ""imageBackgroundColor"": ""#FFFFFF"",
                                                ""title"": ""Menu"",
                                                ""text"": ""Please select"",
                                                ""defaultAction"": {
                                                    ""type"": ""uri"",
                                                    ""label"": ""View detail"",
                                                    ""uri"": ""http://example.com/page/123""
                                                },
                                                ""actions"": [
                                                    {
                                                        ""type"": ""postback"",
                                                        ""label"": ""Buy"",
                                                        ""data"": ""action=buy&itemid=123""
                                                    },
                                                    {
                                                        ""type"": ""postback"",
                                                        ""label"": ""Add to cart"",
                                                        ""data"": ""action=add&itemid=123""
                                                    },
                                                    {
                                                        ""type"": ""uri"",
                                                        ""label"": ""View detail"",
                                                        ""uri"": ""http://example.com/page/123""
                                                    }
                                                ]
                                            }
                                        }
                                    ]";
                            bot.ReplyMessageWithJSON(item.replyToken, Flex);
                            ////建立actions，作為ButtonTemplate的用戶回覆行為
                            //var actions = new List<isRock.LineBot.TemplateActionBase>();
                            //actions.Add(new isRock.LineBot.MessageAction()
                            //{ label = "點選這邊等同用戶直接輸入某訊息", text = "/例如這樣" });
                            //actions.Add(new isRock.LineBot.UriAction()
                            //{ label = "點這邊開啟網頁", uri = new Uri("http://www.google.com") });
                            //actions.Add(new isRock.LineBot.PostbackAction()
                            //{ label = "點這邊發生postback", data = "123"});
                            ////單一Button Template Message
                            //var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
                            //{
                            //    altText = "替代文字(在無法顯示Button Template的時候顯示)",
                            //    text = "text",
                            //    title = "title",
                            //    //設定圖片
                            //    //thumbnailImageUrl = new  Uri("https://scontent-tpe1-1.xx.fbcdn.net/v/t31.0-8/15800635_1324407647598805_917901174271992826_o.jpg?oh=2fe14b080454b33be59cdfea8245406d&oe=591D5C94"),
                            //    actions = actions //設定回覆動作
                            //};
                            ////發送
                            //bot.PushMessage(targetId, ButtonTemplate);

                            //if (ReceivedMessage.events[0].postback != null)
                            //{
                            //    Message += postData;
                            //}
                            //isRock.LineBot.Utility.ReplyMessage(
                            //    ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);
                        }
                        else if (ReceivedMessage.events[0].message.text == "查詢清單")
                        {
                            //var LiffURL = "https://buytheway.000webhostapp.com/"; //測試用位置
                            //建立LiffApp
                            //var Liff = isRock.LIFF.Utility.AddLiffApp(
                            //    MyLineChannelAccessToken, new Uri(LiffURL), isRock.LIFF.ViewType.compact);
                            data.Add("groupid", targetId);
                            json = JsonConvert.SerializeObject(data);
                            //var Liff = "line://app/1651306906-NWLA8wl2" + json;
                            
                            //建立actions，作為ButtonTemplate的用戶回覆行為
                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            actions.Add(new isRock.LineBot.UriAction()
                            { label = "點這邊開啟網頁", uri = new Uri(HttpUtility.UrlEncode(json)) });
                            //單一Button Template Message
                            var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
                            {
                                altText = "替代文字(在無法顯示Button Template的時候顯示)",
                                text = "text",
                                title = "已為您查詢",
                                //設定圖片
                                //thumbnailImageUrl = new  Uri("https://scontent-tpe1-1.xx.fbcdn.net/v/t31.0-8/15800635_1324407647598805_917901174271992826_o.jpg?oh=2fe14b080454b33be59cdfea8245406d&oe=591D5C94"),
                                actions = actions //設定回覆動作
                            };
                            //發送
                            bot.PushMessage(targetId, ButtonTemplate);

                            if (ReceivedMessage.events[0].postback != null)
                            {
                                Message += postData;
                            }
                            isRock.LineBot.Utility.ReplyMessage(
                                ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);
                        }
                        else if (ReceivedMessage.events[0].message.text == "ConfirmTemplate")
                        {
                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            string info = 123 + "|" + 456;
                            actions.Add(new isRock.LineBot.PostbackAction()
                            { label = "YES", data = info + "|Y" });
                            actions.Add(new isRock.LineBot.PostbackAction()
                            { label = "NO", data = info + "|N" });

                            var ConfirmTemplate = new isRock.LineBot.ConfirmTemplate()
                            {
                                text = "msg",
                                actions = actions //設定回覆動作
                            };
                            bot.PushMessage(targetId, ConfirmTemplate);

                            if (ReceivedMessage.events[0].postback != null)
                            {
                                Message += postData;
                            }
                            isRock.LineBot.Utility.ReplyMessage(
                                ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);
                            //isRock.LineBot.Utility.PushTemplateMessage(targetId, ConfirmTemplate, MyLineChannelAccessToken);
                        }
                        else if (ReceivedMessage.events[0].message.text == "CarouselTemplate")
                        {
                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            actions.Add(new isRock.LineBot.MessageAction() { label = "標題-文字回覆", text = "回覆文字" });
                            actions.Add(new isRock.LineBot.UriAction() { label = "標題-Google", uri = new Uri("http://www.google.com") });
                            actions.Add(new isRock.LineBot.PostbackAction() { label = "標題-發生postack", data = "abc=aaa&def=111" });
                            //單一Column
                            var Column = new isRock.LineBot.Column
                            {
                                text = "ButtonsTemplate文字訊息",
                                title = "ButtonsTemplate標題",
                                //設定圖片
                                thumbnailImageUrl = new Uri("https://arock.blob.core.windows.net/blogdata201706/22-124357-ad3c87d6-b9cc-488a-8150-1c2fe642d237.png"),
                                actions = actions //設定回覆動作
                            };
                            //建立CarouselTemplate
                            var CarouselTemplate = new isRock.LineBot.CarouselTemplate();
                            //這是範例，所以用一組樣板建立三個
                            CarouselTemplate.columns.Add(Column);
                            CarouselTemplate.columns.Add(Column);
                            CarouselTemplate.columns.Add(Column);
                            //發送 CarouselTemplate
                            bot.PushMessage(targetId, CarouselTemplate);

                            if (ReceivedMessage.events[0].postback != null)
                            {
                                Message += postData;
                            }
                            isRock.LineBot.Utility.ReplyMessage(
                                ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);
                        }
                        else if (ReceivedMessage.events[0].message.text == "ImageCarouselTemplate")
                        {
                            //第一個Column
                            var ImageCarouselColumn1 = new isRock.LineBot.ImageCarouselColumn
                            {
                                //設定圖片
                                imageUrl = new Uri("https://arock.blob.core.windows.net/blogdata201706/22-124357-ad3c87d6-b9cc-488a-8150-1c2fe642d237.png"),
                                //設定回覆動作
                                action = new isRock.LineBot.MessageAction() { label = "標題A", text = "回覆文字A" }
                            };
                            //第一個Column
                            var ImageCarouselColumn2 = new isRock.LineBot.ImageCarouselColumn
                            {
                                //設定圖片
                                imageUrl = new Uri("https://arock.blob.core.windows.net/blogdata201803/29-101326-d653db4b-44ea-4fe9-af6b-26730734d450.png"),
                                //設定回覆動作
                                action = new isRock.LineBot.MessageAction() { label = "標題B", text = "回覆文字B" }
                            };
                            //建立CarouselTemplate
                            var ImageCarouselTemplate = new isRock.LineBot.ImageCarouselTemplate();
                            //這是範例，所以用一組樣板建立三個
                            ImageCarouselTemplate.columns.Add(ImageCarouselColumn1);
                            ImageCarouselTemplate.columns.Add(ImageCarouselColumn2);
                            //發送 CarouselTemplate
                            bot.PushMessage(targetId, ImageCarouselTemplate);
                        }
                        else if (ReceivedMessage.events[0].message.text == "datepicker")
                        {
                            //建立actions，作為ButtonTemplate的用戶回覆行為
                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            actions.Add(new isRock.LineBot.DateTimePickerAction()
                            {
                                label = "測試-選取時間",
                                mode = "time"
                            });
                            actions.Add(new isRock.LineBot.DateTimePickerAction()
                            {
                                label = "測試-選取日期",
                                mode = "date"
                            });
                            actions.Add(new isRock.LineBot.DateTimePickerAction()
                            {
                                label = "測試-選取時間日期",
                                mode = "datetime"
                            });

                            //單一Button Template Message
                            var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
                            {
                                text = "這個範例測試使用Line新釋出的DateTime Action，讓用戶選擇時間日期並取得會傳值...",
                                title = "ButtonsTemplate測試",
                                //設定圖片
                                thumbnailImageUrl = new Uri("https://arock.blob.core.windows.net/blogdata201706/22-124357-ad3c87d6-b9cc-488a-8150-1c2fe642d237.png"),
                                actions = actions //設定回覆動作
                            };
                            //發送
                            bot.PushMessage(targetId, ButtonTemplate);

                            if (ReceivedMessage.events[0].message != null && !string.IsNullOrEmpty(ReceivedMessage.events[0].message.text))
                                Message = "你說了:" + ReceivedMessage.events[0].message.text;

                            if (ReceivedMessage.events[0].postback != null)
                            {
                                Message += "收到 postback data " + ReceivedMessage.events[0].postback.data;
                                Message += "\n postback params(datetime) " +
                                    ReceivedMessage.events[0].postback.Params.datetime;
                                Message += "\n postback params(date) " +
                                    ReceivedMessage.events[0].postback.Params.date;
                                Message += "\n postback params(time) " +
                                    ReceivedMessage.events[0].postback.Params.time;
                                Message += postData;
                            }
                            //回覆用戶
                            isRock.LineBot.Utility.ReplyMessage(
                                ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);
                        }
                        else if (ReceivedMessage.events[0].message.text == "web")
                        {
                            /*var LiffURL = "https://buytheway.000webhostapp.com/"; //測試用位置
                                                                                  //建立LiffApp
                            var Liff = isRock.LIFF.Utility.AddLiffApp(
                                MyLineChannelAccessToken, new Uri(LiffURL), isRock.LIFF.ViewType.compact);
                            //顯示建立好的 Liff App*/
                            data.Add("userid", "U85dcd98d7d3c827c6b07bc54dd08203a");
                            json = JsonConvert.SerializeObject(data);
                            isRock.LineBot.Utility.PushMessage(
                                targetId, "test_Liff App 已新增 line://app/1651306906-5dmAPJKv?data=" + json, MyLineChannelAccessToken);

                            isRock.LineBot.Utility.PushMessage(
                                targetId, "test_Liff App 已新增 line://app/1651306906-NWLA8wl2", MyLineChannelAccessToken);

                        }
                        else if (ReceivedMessage.events[0].message.text == "registered")
                        {
                            string registered = @"
                                    [
                                        {
                                            ""type"": ""template"",
                                            ""altText"": ""This is a buttons template"",
                                            ""template"": {
                                                ""type"": ""buttons"",
                                                ""imageAspectRatio"": ""rectangle"",
                                                ""imageSize"": ""cover"",
                                                ""imageBackgroundColor"": ""#FFFFFF"",
                                                ""title"": ""Menu"",
                                                ""text"": ""Please select"",
                                                ""defaultAction"": {
                                                    ""type"": ""uri"",
                                                    ""label"": ""View detail"",
                                                    ""uri"": ""http://example.com/page/123""
                                                },
                                                ""actions"": [
                                                    {
                                                        ""type"": ""uri"",
                                                        ""label"": ""👉🏻查詢請點擊👈🏻"",
                                                        ""uri"": ""line://app/1651306906-NWLA8wl2?groupid=C028587cd0cbbbe6cf026bbb0b1e86aae""
                                                    }
                                                ]
                                            }
                                        }
                                    ]";
                            bot.ReplyMessageWithJSON(item.replyToken, registered);
                        }
                        else if (ReceivedMessage.events[0].message.type == "sticker")        //傳送貼圖
                        {
                            Message = $"現在時間:{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")} 你傳送了貼圖";
                            //bot.PushMessage(targetId, 1, 1);
                            //回覆用戶
                            //bot.PushMessage(targetId, Message);
                        }
                        else if (ReceivedMessage.events[0].source.type == "group" && ReceivedMessage.events[0].message.text == "liff")
                        {
                            data.Add("groupid", targetId);
                            data.Add("requesterid", "Ud93750747209cc851468a1707394c401");
                            data.Add("purchaserid", "Ud93750747209cc851468a1707394c401");
                            data.Add("thing", "車票");
                            json = JsonConvert.SerializeObject(data);
                            string Flex = @"
                                    [
                                        {
                                            ""type"": ""template"",
                                            ""altText"": ""This is a buttons template"",
                                            ""template"": {
                                                ""type"": ""buttons"",
                                                ""imageAspectRatio"": ""rectangle"",
                                                ""imageSize"": ""cover"",
                                                ""imageBackgroundColor"": ""#FFFFFF"",
                                                ""title"": ""Menu"",
                                                ""text"": ""Please select"",
                                                ""defaultAction"": {
                                                    ""type"": ""uri"",
                                                    ""label"": ""View detail"",
                                                    ""uri"": ""http://example.com/page/123""
                                                },
                                                ""actions"": [
                                                    {
                                                        ""type"": ""uri"",
                                                        ""label"": ""👉🏻查詢請點擊👈🏻"",
                                                        ""uri"": ""line://app/1651306906-zbeKmqB3?data="+ HttpUtility.UrlEncode(json)+@"""
                                                    }
                                                ]
                                            }
                                        }
                                    ]";
                            bot.ReplyMessageWithJSON(item.replyToken, Flex);
                        }
                        else if (ReceivedMessage.events[0].source.type == "user" && ReceivedMessage.events[0].message.text == "liff")
                        {
                            string Flex = @"
                                    [
                                        {
                                            ""type"": ""template"",
                                            ""altText"": ""This is a buttons template"",
                                            ""template"": {
                                                ""type"": ""buttons"",
                                                ""imageAspectRatio"": ""rectangle"",
                                                ""imageSize"": ""cover"",
                                                ""imageBackgroundColor"": ""#FFFFFF"",
                                                ""title"": ""Menu"",
                                                ""text"": ""Please select"",
                                                ""defaultAction"": {
                                                    ""type"": ""uri"",
                                                    ""label"": ""View detail"",
                                                    ""uri"": ""http://example.com/page/123""
                                                },
                                                ""actions"": [
                                                    {
                                                        ""type"": ""uri"",
                                                        ""label"": ""👉🏻查詢請點擊👈🏻"",
                                                        ""uri"": ""line://app/1651306906-brGm9MPjdata=" + HttpUtility.UrlEncode(json) + @"""
                                                    }
                                                ]
                                            }
                                        }
                                    ]";
                            bot.ReplyMessageWithJSON(item.replyToken, Flex);
                        }
                    }
                }
                else if(ReceivedMessage.events[0].type == "postback")
                {
                    /*data = JsonConvert.DeserializeObject<Dictionary<string,string>>(item.postback.data);
                    data.Add("group", "群");
                    data.Add("touch", item.source.userId);
                    json = JsonConvert.SerializeObject(data);
                    string liff_url = "line://app/1609433400-QqA8q0V4?data=" + HttpUtility.UrlEncode(json);
                    if (ReceivedMessage.events[0].postback.data == "我")
                    {   //此user的資料+群組內有此user資料
                        //bot.PushMessage(targetId, ReceivedMessage.events[0].source.userId);
                        data.Add("userid", targetId);
                        json = JsonConvert.SerializeObject(data);
                        bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                        bot.PushMessage(targetId, "done");
                    }
                    if (ReceivedMessage.events[0].postback.data == "群組")   //群組內的所有資料
                    {
                        data.Add("groupid", targetId);
                        json = JsonConvert.SerializeObject(data);
                        bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                        bot.PushMessage(targetId, "done");
                    }
                    if (ReceivedMessage.events[0].postback.data == "全部")     //群組資料與個人資料
                    {
                        data.Add("userid", targetId);
                        data.Add("groupid", targetId);
                        json = JsonConvert.SerializeObject(data);
                        bot.PushMessage(targetId, GetHttpData($"https://buytheway.000webhostapp.com/CRUD/R_select.php?data={json}"));
                        bot.PushMessage(targetId, "done");
                    }*/
                    //isRock.LineBot.Utility.ReplyMessage(
                    //            ReceivedMessage.events[0].replyToken, postData, MyLineChannelAccessToken);

                    string Flex = @"
                                    [
                                        {
                                            ""type"": ""template"",
                                            ""altText"": ""This is a buttons template"",
                                            ""template"": {
                                                ""type"": ""buttons"",
                                                ""imageAspectRatio"": ""rectangle"",
                                                ""imageSize"": ""cover"",
                                                ""imageBackgroundColor"": ""#FFFFFF"",
                                                ""title"": ""Menu"",
                                                ""text"": ""Please select"",
                                                ""defaultAction"": {
                                                    ""type"": ""uri"",
                                                    ""label"": ""View detail"",
                                                    ""uri"": ""http://example.com/page/123""
                                                },
                                                ""actions"": [
                                                    {
                                                        ""type"": ""postback"",
                                                        ""label"": ""Buy"",
                                                        ""data"": ""action=buy&itemid=123""
                                                    },
                                                    {
                                                        ""type"": ""postback"",
                                                        ""label"": ""Add to cart"",
                                                        ""data"": ""action=add&itemid=123""
                                                    },
                                                    {
                                                        ""type"": ""uri"",
                                                        ""label"": ""View detail"",
                                                        ""uri"": ""http://example.com/page/123""
                                                    }
                                                ]
                                            }
                                        }
                                    ]";
                    bot.ReplyMessageWithJSON(item.replyToken, Flex);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                isRock.LineBot.Utility.ReplyMessage(
                            ReceivedMessage.events[0].replyToken, ex.ToString(), MyLineChannelAccessToken);
                return Ok();
            }
        }
        public static string GetHttpData(string Url)    //取特定網站
        {
            string sException = null;
            string sRslt = null;
            WebResponse oWebRps = null;
            WebRequest oWebRqst = WebRequest.Create(Url);
            oWebRqst.Timeout = 50000;
            try
            {
                oWebRps = oWebRqst.GetResponse();
            }
            catch (WebException e)
            {
                sException = e.Message.ToString();
            }
            catch (Exception e)
            {
                sException = e.ToString();
            }
            finally
            {
                if (oWebRps != null)
                {
                    StreamReader oStreamRd = new StreamReader(oWebRps.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                    sRslt = oStreamRd.ReadToEnd();
                    oStreamRd.Close();
                    oWebRps.Close();
                }
            }
            return sRslt;
        }

        public static byte[] ImageToBuffer(Image Image, System.Drawing.Imaging.ImageFormat imageFormat)     //將image轉成byte
        {
            if (Image == null) { return null; }
            byte[] data = null;
            using (MemoryStream oMemoryStream = new MemoryStream())
            {
                //建立副本
                using (Bitmap oBitmap = new Bitmap(Image))
                {
                    //儲存圖片到 MemoryStream 物件，並且指定儲存影像之格式
                    oBitmap.Save(oMemoryStream, imageFormat);
                    //設定資料流位置
                    oMemoryStream.Position = 0;
                    //設定 buffer 長度
                    data = new byte[oMemoryStream.Length];
                    //將資料寫入 buffer
                    oMemoryStream.Read(data, 0, Convert.ToInt32(oMemoryStream.Length));
                    //將所有緩衝區的資料寫入資料流
                    oMemoryStream.Flush();
                }
            }
            return data;
        }
        
    }
}
