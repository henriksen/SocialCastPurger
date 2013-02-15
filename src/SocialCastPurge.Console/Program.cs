using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Xml;
using SocialCastLib;

namespace SocialCastPurge.Purger
{
    class Program
    {

        static void Main(string[] args)
        {
            Credentials.ReadCredentials();

            InitAD();

            var auth = new SocialCastAuthDetails()
            {
                DomainName = Credentials.ScDomain,
                Username = Credentials.ScUsername,
                Password = Credentials.ScPassword
            };

            int page = 1;
            var xDoc = new APIAccessor().GetUsers("10", page.ToString(), auth);
            var quitterCount = 0;
            var usersPrSecondMovingAverage = new RRQueue(20);
            Console.Clear();
            var startTime = DateTime.Now;
            //	var _xDoc = new APIAccessor().GetUsers("niall", auth);
            var usersNode = xDoc.SelectSingleNode("users");
            bool usersLeft = usersNode != null && usersNode.HasChildNodes;
            using (var quitters = System.IO.File.CreateText("quitters.csv"))
            {
                while (usersLeft)
                {
                    int origRow = Console.CursorTop;
                    int origCol = Console.CursorLeft;
                    int index = 1;
                    foreach (XmlNode item in xDoc.GetElementsByTagName("user"))
                    {

                        //item.Dump();
                        string id = item.SelectSingleNode("id").InnerText;
                        string email = item.SelectSingleNode("contact-info/email").InnerText;
                        //bool terminated = bool.Parse(item.SelectSingleNode("terminated").InnerText);
                        string username = item.SelectSingleNode("username").InnerText;

                        Console.SetCursorPosition(0, 0);
                        var count = index + (page - 1)*10;
                        var percent = (int) (index/1373.0*100);
                        var seconds = (DateTime.Now - startTime).TotalSeconds;
                        var usersPrSecond = (count/seconds);
                        usersPrSecondMovingAverage.Enqueue(usersPrSecond);
                        var eta = DateTime.Now.AddSeconds((1373 - count) / usersPrSecondMovingAverage.Average);
                        Console.WriteLine("Tested {0} users of ~1373 ({1} %). {2:F2} users / sec. ETA {3}  ",
                            count, percent, usersPrSecondMovingAverage.Average, eta);
                        Console.Write("Found {0} quitters", quitterCount);

                        if (!DoesUserExist(email))
                        {
                            quitterCount++;
                            quitters.WriteLine("{0}; {1}; {2}; finnes ikke i AD", id, email, username);
                            quitters.Flush();
                        }
                        index++;
                    }
                    page++;
                    xDoc = new APIAccessor().GetUsers("10", page.ToString(), auth);
                    usersNode = xDoc.SelectSingleNode("users");
                    usersLeft = usersNode != null && usersNode.HasChildNodes;
                }
            }
            Console.ReadKey(true);

        }

        static DirectoryEntry _de;
        static DirectorySearcher _ds;


        static void InitAD()
        {
            string ldapAddress = "LDAP://" + Credentials.AdServer;
            _de = new DirectoryEntry(ldapAddress, Credentials.AdUsername, Credentials.AdPassword);
            _ds = new DirectorySearcher(_de) {SearchScope = SearchScope.Subtree};
        }

        static bool DoesUserExist(string email)
        {
            return FindUserByEmail(email) != null;
        }

        static SearchResult FindUserByEmail(string email)
        {
            _ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(proxyAddresses=" + "smtp:" + email + "))";
            SearchResult rs = _ds.FindOne();
            return rs;
        }

    }
}
