using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace BotEngine.QnA
{

    public class QnAMakerResult
    {
        [JsonProperty(PropertyName = "answers")]
        public List<Result> Answers { get; set; }
    }

    public class Result
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "questions")]
        public List<string> Questions { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }


    public class QuestionAnswerMaker
    {
        public static List<string> GetAnswer(string query)
        {
            string responseString = string.Empty;

            try
            {
                //var knowledgebaseId =  Convert.ToString("2a7fb080-8b30-426a-b8ee-f8bc5abb71b1", CultureInfo.InvariantCulture); -- uday
                //var knowledgebaseId = Convert.ToString("b2561a18-9fe2-46a1-bf9d-dc6beb001095", CultureInfo.InvariantCulture); -- yash
                var knowledgebaseId = Convert.ToString("49cc4a74-3e50-4205-a1ea-b4444104201e", CultureInfo.InvariantCulture); //-- nikhil
                                                                                                                              //var knowledgebaseId = Convert.ToString("127320ac-6a2d-4901-a493-d86e8b1e9660", CultureInfo.InvariantCulture);

        //Build the URI
        //var builder = new UriBuilder(string.Format(Convert.ToString($"https://allwayshealth-it-bot-d01-qna.azurewebsites.net/qnamaker/knowledgebases/{knowledgebaseId}/generateAnswer", CultureInfo.InvariantCulture), knowledgebaseId)); 
        var builder = new UriBuilder(string.Format(Convert.ToString($"https://botqnamakerdemo.azurewebsites.net/qnamaker/knowledgebases/{knowledgebaseId}/generateAnswer", CultureInfo.InvariantCulture), knowledgebaseId)); //Nikhil

        //Add the question as part of the body
        var postBody = string.Format("{{\"question\": \"{0}\",\"top\": \"{1}\"}}", query, 10);

                //Send the POST request
                using (WebClient client = new WebClient())
                {
                    //Set the encoding to UTF8
                    client.Encoding = System.Text.Encoding.UTF8;


                    //Add the subscription key header
                    //var qnamakerSubscriptionKey = Convert.ToString("63a32ec5-7201-4ac3-8fb6-823bf795302f", CultureInfo.InvariantCulture); -- yash
                    //var qnamakerSubscriptionKey = Convert.ToString("28610e60-e22c-4398-a844-2c81ea3a24fa", CultureInfo.InvariantCulture); -- uday
                    var qnamakerSubscriptionKey = Convert.ToString("14626a6f-2922-4830-84c8-78839ce09e89", CultureInfo.InvariantCulture); //-- nikhil
                    //var qnamakerSubscriptionKey = Convert.ToString("4539d041-02aa-49f9-a1b7-f925ad585013", CultureInfo.InvariantCulture);

                    client.Headers.Add("Authorization", $"EndpointKey {qnamakerSubscriptionKey}");
                    client.Headers.Add("Content-Type", "application/json");
                    responseString = client.UploadString(builder.Uri, postBody);
                }
                QnAMakerResult result = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);

                List<string> answers = new List<string>();
                //HashSet<string> uniqueAnswers = new HashSet<string>();

                result.Answers.ForEach(x =>
                                        {
                                            answers.Add(x.Answer);

                                    //uniqueAnswers.Add(x.Answer);
                                });

                answers = answers.Distinct().ToList();

                return answers;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return null;
        }
    }
}