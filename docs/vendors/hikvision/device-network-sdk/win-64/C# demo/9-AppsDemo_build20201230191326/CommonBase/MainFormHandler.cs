using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public class MainFormHandler
    {
        public static event Action<Level, string, string, string> SetStatus;
        public static Form MainFormInstance
        {
            set
            {
                m_pMainFormHandler = value;
            }
        }

        public enum Level
        {
            Debug = 0,
            Info,
            Error
        }

        private static Form m_pMainFormHandler = null;
        public static bool SetStatusString(Level level, string module, string message, string details = "")
        {
            if (m_pMainFormHandler != null)
            {
                SetStatus(level, module, message, details);
                return true;
            }
            return false;
        }
    }
}
