using System;
using NHibernate;
using SocketServer.DBMapping;
using System.Collections;
using NHibernate.Cfg;

namespace SocketServer
{
    class DataBase
    {
        private Configuration config;
        private ISessionFactory factory;

        //Constructor
        public DataBase()
        {
            //setup for the rest of the connection for the rest of the class
            config = new Configuration();
            config.Configure();
            config.AddAssembly("SocketServer");
            factory = config.BuildSessionFactory();
        }

        //test write
        private void TestWrite()
        {
            Console.WriteLine("starting initializeation");
            try
            {
                User usr = new User();
                using (ISession session = factory.OpenSession())
                {
                    usr.Name = "testName";
                    session.SaveOrUpdate(usr);
                    session.Flush();
                    session.Close();
                }
                factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //test retreaveal
        public void TestRead()
        {
            Console.WriteLine("starting read");
            try
            {
                IList usrs;
                using (ISession session = factory.OpenSession())
                {
                    //NHibernate setup for retreaving a list.
                    ICriteria sc = session.CreateCriteria(typeof(User));
                    usrs = sc.List();
                    session.Close();
                    //used to prove users were gotten
                    foreach (User usr in usrs)
                    {
                        Console.WriteLine(usr.Name);
                    }
                }
                factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //test retreaveal of single
        public void TestSingleReadWithID()
        {
            Console.WriteLine("starting single read with ID");
            try
            {
                User usr;
                using (ISession session = factory.OpenSession())
                {
                    //NHibernate setup for retreaving a single item
                    //usr = (User)session.Load(typeof(User), 1);
                    usr = (User)session.Get(typeof(User), 22);
                    if (usr != null)// if not found it will be null
                        Console.WriteLine(usr.Name);
                    else
                        Console.WriteLine("not found");
                    session.Close();
                }
                factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //test retreaveal of single with username
        public void TestSingleRead()
        {
            Console.WriteLine("starting single read");
            try
            {
                User usr;
                using (ISession session = factory.OpenSession())
                {
                    //NHibernate setup for retreaving a single item
                    //gets by different field.
                    //pre built query
                    var q = session.CreateQuery("from User u where u.Name=:userName");
                    q.SetMaxResults(1);
                    q.SetString("userName", "testName");
                    //runs the pre built query
                    usr = q.UniqueResult<User>();
                    if (usr != null)// if not found it will be null
                        Console.WriteLine(usr.Name);
                    else
                        Console.WriteLine("not found");
                    session.Close();
                }
                factory.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
