using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotConverter.Classes
{
    class AppException : Exception
    {
        public AppException(string message) : base(message)
        {
        }
    }
}
