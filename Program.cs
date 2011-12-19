using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using JsonFx.Json;
using System.IO;

namespace GitHubStats
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: GitHubStats <username> <password>");
                return;
            }

            string jsonResult1;
            try
            {
                WebClient client = new WebClient();
                client.Credentials = new NetworkCredential(args[0], args[1]);

                string jsonResult0 = client.DownloadString("https://api.github.com/user/emails");
                dynamic result0 = new JsonReader().Read(jsonResult0);

                string[] emails = (string[])result0;

                jsonResult1 = client.DownloadString("https://api.github.com/users/" + args[0] + "/repos?per_page=100");
                dynamic result1 = new JsonReader().Read(jsonResult1);

                int repoCount = 0;
                int commitCount = 0;
                int changesCount = 0;

                foreach (var r in result1)
                {
                    Console.WriteLine("Inspecting " + r.name + "...");

                    DateTime createdDate1 = Convert.ToDateTime(r.created_at);
                    if (createdDate1 > DateTime.Now.AddYears(-1))
                    {
                        repoCount++;
                    }

                    try
                    {
                        int page = 1;
                        string jsonResult2;
                        do
                        {
                            jsonResult2 = client.DownloadString("http://github.com/api/v2/json/commits/list/" + args[0] + "/" + r.name + "/master?page=" + page);
                            dynamic result2 = new JsonReader().Read(jsonResult2);

                            foreach (var c in result2.commits)
                            {
                                DateTime createdDate2 = Convert.ToDateTime(c.committed_date);
                                if (createdDate2 > DateTime.Now.AddYears(-1) &&
                                    (emails.Contains((string)c.author.email) == true ||
                                     emails.Contains((string)c.committer.email) == true))
                                {
                                    commitCount++;

                                    string jsonResult3 = client.DownloadString("https://api.github.com/repos/" + args[0] + "/" + r.name + "/commits/" + c.id);
                                    dynamic result3 = new JsonReader().Read(jsonResult3);

                                    changesCount = changesCount + result3.stats.total;
                                }
                            }

                            page++;
                        } while (jsonResult2 != null);

                        Console.WriteLine("Done.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Done.");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("You created " + repoCount + " repositories in the last year.");
                Console.WriteLine("You created " + commitCount + " commits in the last year.");
                Console.WriteLine("You created " + changesCount + " lines of code in the last year.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
