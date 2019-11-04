using BcrServer_Repository;

namespace BcrServer
{
    public class App500
    {
        private static App500 instance;
        public static App500 Instance
        {
            get
            {
                if (instance == null)
                    instance = new App500();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        UnitOfWork uow;
        public App500()
        {
            uow = new UnitOfWork();
        }

        ~App500()
        {
            uow = null;
        }

    }
}
