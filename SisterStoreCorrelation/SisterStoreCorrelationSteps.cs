using System;
using System.Collections.Generic;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SisterStoreCorrelation
{
    [Binding]
    public class SisterStoreCorrelationSteps
    {

        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;
        private string _appActualName;
        private dynamic statusresp;
       
        private Client Obj = new Client("https://gallery.alteryx.com/api");

        private RootObject jsString = new RootObject();

        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string SUT_url)
        {
            alteryxurl = Environment.GetEnvironmentVariable(SUT_url);
        }
        
        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            _sessionid = Obj.Authenticate(user, password).sessionId;
        }
        
        [When(@"I run the app ""(.*)"" with the details of drive time trade areas ""(.*)"",""(.*)"",""(.*)"",""(.*)"",""(.*)""")]
        public void WhenIRunTheAppWithTheDetailsOfDriveTimeTradeAreas(string app, int superurban, int urban, int suburban, int exurban, int rural)
        {
          
            //url + "/apps/gallery/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 
            string response = Obj.SearchAppsGallery(app);
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            if (count == 1)
            {
                _appid = appresponse["records"][0]["id"];
                _userid = appresponse["records"][0]["owner"]["id"];
                _appName = appresponse["records"][0]["primaryApplication"]["fileName"];
            }
            else
            {
                for (int i = 0; i <= count - 1; i++)
                {

                    _appActualName = appresponse["records"][i]["primaryApplication"]["metaInfo"]["name"];
                    if (_appActualName == app)
                    {
                        _appid = appresponse["records"][i]["id"];
                        _userid = appresponse["records"][i]["owner"]["id"];
                        _appName = appresponse["records"][i]["primaryApplication"]["fileName"];
                        break;
                    }
                }

            }
            jsString.appPackage.id = _appid;
            jsString.userId = _userid;
            jsString.appName = _appName;

            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);
            
            List<JsonPayload.Question> questionAnsls1 = new List<JsonPayload.Question>();
            questionAnsls1.Add(new JsonPayload.Question("Super Urban", "\"" + superurban+ "\""));
            questionAnsls1.Add(new JsonPayload.Question("Urban", "\"" + urban + "\""));
            questionAnsls1.Add(new JsonPayload.Question("Suburban", "\"" + suburban + "\""));
            questionAnsls1.Add(new JsonPayload.Question("Exurban", "\"" + exurban + "\""));
            questionAnsls1.Add(new JsonPayload.Question("Rural", "\"" + rural + "\""));
            jsString.questions.AddRange(questionAnsls1);
        }
        
        [When(@"I specify the Potential Site Location """"(.*)"""", ""(.*)"", ""(.*)"", """"(.*)""""")]
        public void WhenISpecifyThePotentialSiteLocation(string address, string city , string state, int zip)
        {
            List<JsonPayload.Question> questionAnsls2 = new List<JsonPayload.Question>();
            questionAnsls2.Add(new JsonPayload.Question("Address 2", "\"" + address + "\""));
            questionAnsls2.Add(new JsonPayload.Question("City 2", "\"" + city + "\""));
            questionAnsls2.Add(new JsonPayload.Question("State 2", "\"" + state + "\""));
            questionAnsls2.Add(new JsonPayload.Question("ZIP 2", "\""+zip.ToString()+"\""));
            jsString.questions.AddRange(questionAnsls2);
            
            //Construct the payload to be posted.
            jsString.jobName = "Job Name";

            // Make Call to run app

            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];

            string status = "";
            while (status != "Completed")
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                statusresp =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusresp["status"];
            }
        }
        
        [Then(@"I see the output contains the text ""(.*)""")]
        public void ThenISeeTheOutputContainsTheText(string output)
        {
             //url + "/apps/jobs/" + jobId + "/output/"
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            // outputid = metadataresp[0]["id"];
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }

            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string response = doc.DocumentNode.SelectSingleNode("//div[@class='DefaultText']").InnerText;  
            StringAssert.Contains(output,response);
        }          
   }
}

