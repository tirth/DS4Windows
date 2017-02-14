using System;
using System.ComponentModel;

namespace DS4Lib.Control 
{
    public partial class ScpHub : Component 
    {
        protected IntPtr m_Reference = IntPtr.Zero;
        protected volatile bool m_Started = false;

        public event EventHandler<DebugEventArgs>   Debug;
       
        public event EventHandler<ReportEventArgs>  Report;

        protected virtual bool LogDebug(string Data, bool warning) 
        {
            var args = new DebugEventArgs(Data, warning);

            On_Debug(this, args);

            return true;
        }

        public bool Active 
        {
            get { return m_Started; }
        }


        public ScpHub() 
        {
            InitializeComponent();
        }

        public ScpHub(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public virtual bool Open()  
        {
            return true;
        }

        public virtual bool Start() 
        {
            return m_Started;
        }

        public virtual bool Stop()  
        {
            return !m_Started;
        }

        public virtual bool Close() 
        {
            if (m_Reference != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Reference);

            return !m_Started;
        }


        public virtual bool Suspend() 
        {
            return true;
        }

        public virtual bool Resume()  
        {
            return true;
        }

        protected virtual void On_Debug(object sender, DebugEventArgs e)     
        {
            if (Debug != null) Debug(sender, e);
        }


        protected virtual void On_Report(object sender, ReportEventArgs e)   
        {
            if (Report != null) Report(sender, e);
        }
    }
}
