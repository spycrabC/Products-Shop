using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DAY1
{
    internal class Patterns
    {
        public static bool PasswordCheck(string password)
        {
            if (password.Length > 8)
            {
                if (Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool EmailCheck(string email)
        {
            
                if (Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }

        public static bool NumberCheck(string number)
        {

            if (Regex.IsMatch(number, @"^(?:\+7|8)\d{10}$"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
