
namespace SocketServer
{
    using System;
    using System.Collections;

    using NHibernate;
    using NHibernate.Cfg;

    using SocketServer.DBMapping;

    internal class DataBase
    {
        private readonly Configuration config;

        private readonly ISessionFactory factory;

        // Constructor
        public DataBase()
        {
            // setup for the rest of the connection for the rest of the class
            this.config = new Configuration();
            this.config.Configure();
            this.config.AddAssembly("SocketServer");
            this.factory = this.config.BuildSessionFactory();
        }

        // test retreaveal
        public void TestRead()
        {
            Console.WriteLine("starting read");
            try
            {
                IList usrs;
                using (var session = this.factory.OpenSession())
                {
                    // NHibernate setup for retreaving a list.
                    var sc = session.CreateCriteria(typeof(User));
                    usrs = sc.List();
                    session.Close();

                    // used to prove users were gotten
                    foreach (User usr in usrs)
                    {
                        Console.WriteLine(usr.Name);
                    }
                }

                this.factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // test retreave of single with username
        public void TestSingleRead()
        {
            Console.WriteLine("starting single read");
            try
            {
                User usr;
                using (var session = this.factory.OpenSession())
                {
                    // NHibernate setup for retreaving a single item
                    // gets by different field.
                    // pre built query
                    var q = session.CreateQuery("from User u where u.Name=:userName");
                    q.SetMaxResults(1);
                    q.SetString("userName", "testName");

                    // runs the pre built query
                    usr = q.UniqueResult<User>();
                    Console.WriteLine(usr != null ? usr.Name : "not found");

                    session.Close();
                }

                this.factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // test retrieving of single
        public void TestSingleReadWithID()
        {
            Console.WriteLine("starting single read with ID");
            try
            {
                User usr;
                using (var session = this.factory.OpenSession())
                {
                    // NHibernate setup for retrieving a single item
                    // usr = (User)session.Load(typeof(User), 1);
                    usr = (User)session.Get(typeof(User), 22);
                    Console.WriteLine(usr != null ? usr.Name : "not found");

                    session.Close();
                }

                this.factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // test write
        private void TestWrite()
        {
            Console.WriteLine("starting initialization");
            try
            {
                var usr = new User();
                using (var session = this.factory.OpenSession())
                {
                    usr.Name = "testName";
                    session.SaveOrUpdate(usr);
                    session.Flush();
                    session.Close();
                }

                this.factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}