using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OnlineChat.Models.Lecturer;
using OnlineChat.Models.Chats;
namespace OnlineChat.Models.Users
{
    public class UserDAO 
    {
        private readonly AESCrypt crypt;
        private LecturerRepository repos;
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IDbConnection connection;
        public UserDAO(IDbConnectionFactory _connectionFactory, AESCrypt _crypt,LecturerRepository _repos)
        {
            connectionFactory = _connectionFactory;
            crypt = _crypt;
            repos = _repos;
            connection = connectionFactory.GetDbConnection();
        }
        public bool AddUser(User user)
        {
            string sqlInsert = "INSERT INTO public.site_user(fio, birth_date, password, email,status) " +
                "VALUES (@FIO, @BDATE, @PAS, @EM,@Status)";


            connection.Execute(sqlInsert, new
            {
                FIO = user.FIO,
                BDATE = user.BirthDate,
                PAS = user.Password,
                EM = user.Email,
                Status=user.Status
            });

            return true;
        }
    
        public User GetUserOnEmail(string Email)
        {
            User user = connection.Query<User>("SELECT * FROM public.site_user WHERE Email = @Email", new { Email }).FirstOrDefault();
            //if (user != null)
            //{
            //    user = crypt.DecryptUser(user);
            //}

            return user;
        }

        public List<Chat> GetChats(int id)
        {
            return connection.Query<Chat>("SELECT C.id AS ID, C.guid AS GUID FROM chats As C Inner Join relation_chats As S on S.chat_id=C.id WHERE S.user_id = @id", new { id }).ToList();

        }
        public String GetOtherUser(int chat_id,int user_id)
        {
            int id = connection.Query<int>("SELECT user_id FROM relation_chats WHERE chat_id = @ChatId and user_id!=@UserId", new { ChatId=chat_id,UserId=user_id }).FirstOrDefault();
            return connection.Query<string>("SELECT fio FROM site_user  WHERE id=@UserId", new {  UserId = id }).FirstOrDefault();
        }
        public User Get(int id)
        {
            User user = connection.Query<User>("SELECT * FROM public.site_user WHERE Id = @id", new { id }).FirstOrDefault();
            if (user != null)
            {
                //user = crypt.DecryptUser(user);
            }
            return user;
        }
        public List<string> GetAllUsers()
        {
            List<string> allUsers = connection.Query<string>("SELECT Email FROM public.site_user WHERE status='Admin'").ToList();
   //         foreach(string user in allUsers)
			//{
   //             crypt.Decrypt(user);
			//}
            return allUsers;


        }
        public List<string> GetAllTeachers()
        {
            List<string> allUsers = connection.Query<string>("SELECT Email FROM public.site_user WHERE status='Teacher'").ToList();
            //         foreach(string user in allUsers)
            //{
            //             crypt.Decrypt(user);
            //}
            return allUsers;


        }
        public List<User> GetAllUsersExceptYourself(string email)
        {
            List<User> allUsersExceptYourself = connection.Query<User>("SELECT * FROM public.site_user WHERE Email!=@email ORDER BY Email", new { email }).ToList();
            foreach (User user in allUsersExceptYourself)
            {
                //crypt.DecryptUser(user);
            }
            return allUsersExceptYourself;
        }
        public bool ChangeStatus(string newStatus, string email)
        {
            
                string sqlCheck = "SELECT id FROM public.site_user WHERE email = @EMAIL";
                string sqlUpd = "UPDATE public.site_user SET status = @NSTAT " +
                    "WHERE email = @EMAIL";

                if (connection.Query<string>(sqlCheck, new { EMAIL = email }).FirstOrDefault() == null)
                {
                    return false;
                }

                if (newStatus != "Teacher" && newStatus != "Student"&& newStatus!="User"&& newStatus != "Admin") return false;

                connection.Execute(sqlUpd, new { EMAIL = email, NSTAT = newStatus });

            if (newStatus == "Teacher")
            {
                User model1;
                model1 = GetUserOnEmail(email);
                LecturerModel model=new LecturerModel();
                model.UserId = model1.Id;
                repos.AddLecturer(model);
            }

                return true;
            
        }
            public List<User> SearchUsers(string text, string email)
        {
            List<User> Users = connection.Query<User>("SELECT * FROM public.site_user WHERE Email!=@em AND Email LIKE @txt ORDER BY Email", new { em=email,txt=text+'%' }).ToList();
            foreach (User user in Users)
            {
                //crypt.DecryptUser(user);
            }
            return Users;

        }
    }
}
