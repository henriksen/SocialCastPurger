using System;
using System.Runtime.InteropServices;
using System.Security;
using SocialCastPurge.Purger.Properties;

namespace SocialCastPurge.Purger
{
    public class Credentials  
    {
        public static string AdUsername { get; set; }

        public static string AdPassword { get; set; }

        public static string ScUsername { get; set; }

        public static string ScPassword { get; set; }

        public static void ReadCredentials()
        {
            
            AdUsername = Settings.Default.adUsername;
            AdPassword = "";
            if (AdUsername == "")
            {
                Console.WriteLine("AD username (with DOMAIN\\):");
                AdUsername = Console.ReadLine();
                Settings.Default.adUsername = AdUsername;
                Settings.Default.Save();
            }
            Console.WriteLine("AD password:");
            AdPassword = ReadPassword();
            Console.WriteLine();

            ScUsername = Settings.Default.scUsername;
            ScPassword = "";
            if (ScUsername == "")
            {
                Console.WriteLine("SocialCast username (email):");
                ScUsername = Console.ReadLine();
                Settings.Default.scUsername = ScUsername;
                Settings.Default.Save();
            }
            Console.WriteLine("SocialCast password:");
            ScPassword = ReadPassword();
            Console.WriteLine();

        }

        static String SecureStringToString(SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        private static string ReadPassword()
        {
            using (var password = new SecureString())
            {
                for (ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                     ConsoleKey.Enter != keyInfo.Key;
                     keyInfo = Console.ReadKey(true))
                {
                    if (ConsoleKey.Backspace == keyInfo.Key)
                    {
                        if (0 < password.Length)
                        {
                            password.RemoveAt(password.Length - 1);
                            Backspace();
                            Console.Write(" ");
                            Backspace();
                        }
                    }
                    else
                    {
                        password.AppendChar(keyInfo.KeyChar);
                        Console.Write("*");
                    }
                }
                return SecureStringToString(password);
            }
        }

        private static void Backspace()
        {
            Console.SetCursorPosition(Console.CursorLeft - 1,
                                      Console.CursorTop);
        }
    }
}