using System;

namespace MapEditor.Controls
{
    public class ErrorEventArgs : EventArgs 
    {
        protected Exception m_ErrorException;

        public Exception ErrorException 
        { 
            get 
            { 
                return m_ErrorException;
            }
            protected set 
            { 
                m_ErrorException = value;
            }
        }

        public ErrorEventArgs(Exception errorExcetion) : base()
        {
            m_ErrorException = errorExcetion;
        }
    }
}
