using MapEditor.Infrastructure;

namespace MapEditor.Models 
{
    public class BaseElementInfo : NotifyObject, IElementInfo 
    {
        #region fields

        private string m_Id;
        private string m_Name;
        private string m_Type;
        private double m_Height;

        #endregion

        #region Properties

        public string Id
        {
            get
            {
                return m_Id;
            }
        }
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                OnPropertyChanged(() => Name);
            }
        }
        public string Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
                OnPropertyChanged(() => Type);
            }
        }
        public double Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                if (m_Height != value)
                {
                    m_Height = value;
                    OnPropertyChanged(() => Height);
                }
            }
        }

        #endregion

        public BaseElementInfo(string id) 
            : this(id, default(string), default(string), default(double)) 
        {
        }
        public BaseElementInfo(string id, string name, string type, double height) 
        {
            m_Id = id;
            m_Name = name;
            m_Type = type;
            m_Height = height;
        }
    }

    public interface IElementInfo 
    { 
        string Id 
        { 
            get;
        }
        string Name 
        { 
            get;
            set;
        }
        string Type 
        { 
            get;
            set;
        }
        double Height 
        { 
            get;
            set;
        }
    }
}
