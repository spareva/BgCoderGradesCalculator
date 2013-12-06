using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetBgCoder
{
    class BgCoderCalculator
    {
        static string username = "";
        static string password = "";
        static string numberOfContest = "144";
        static int numberOfProblems = 5;

        private static void WriteToConsole(string htmlSource, int[] grades)
        {
            int headingStartIndex = htmlSource.IndexOf("<h2>") + 4;
            int headingLastIndex = htmlSource.IndexOf("</h2>");
            string heading = htmlSource.Substring(headingStartIndex, headingLastIndex - headingStartIndex);
            heading = heading.Replace("&quot", "");
            heading = heading.Replace(";", "");
            Console.WriteLine(heading);
            for (int i = 0; i < numberOfProblems; i++)
            {
                Console.WriteLine("Average result for problem " + (i + 1) + " is: " + grades[i]);
            }
            Console.WriteLine();
        }

        private static int[] ExtractGrades(string htmlSource)
        {
            int startIndex = 0;
            int[] grades = new int[numberOfProblems];
            int count = 0;
            bool reachedEnd = false;

            while (true)
            {
                for (int i = 0; i < numberOfProblems; i++)
                {
                    startIndex = htmlSource.IndexOf("<td><center>", startIndex);
                    if (startIndex <= 0)
                    {
                        reachedEnd = true;
                        break;
                    }
                    int endIndex = htmlSource.IndexOf("</center></td>", startIndex + 1);
                    string item = htmlSource.Substring(startIndex, endIndex - startIndex);
                    string result = Regex.Replace(item, @"<[^>]*>", String.Empty);
                    result = result.Replace("\n", "");
                    result = result.Replace("\r", "");
                    grades[i] += int.Parse(result);
                    startIndex = endIndex + 1;
                }
                if (reachedEnd)
                {
                    break;
                }
                count++;
            }

            for (int i = 0; i < numberOfProblems; i++)
            {
                grades[i] = grades[i] / count;
            }

            return grades;
        }

        static void Main()
        {
            var client = new CookieAwareWebClient();
            client.BaseAddress = @"http://bgcoder.com";
            var loginData = new NameValueCollection();
            loginData.Add("UserName", username);
            loginData.Add("Password", password);
            client.UploadValues("/Account/LogOn", "POST", loginData);

            string htmlSource = client.DownloadString("Contest/ContestResults/" + numberOfContest);

            int[] grades = ExtractGrades(htmlSource);

            WriteToConsole(htmlSource, grades);
        }
    }

    public class CookieAwareWebClient : WebClient
    {
        private CookieContainer cookie = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = cookie;
            }
            return request;
        }
    }
}
