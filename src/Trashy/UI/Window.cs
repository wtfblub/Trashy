namespace Trashy.UI
{
    public class Window
    {
        private bool _isOpen;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                OnIsOpenChanged(value);
                _isOpen = value;
            }
        }

        public int Id => GetType().FullName.GetHashCode();

        public virtual void OnDraw()
        {
        }

        protected virtual void OnIsOpenChanged(bool isOpen)
        {
        }
    }
}
