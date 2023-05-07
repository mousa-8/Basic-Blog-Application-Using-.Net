using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIP.Models;
using System.Data;
using System.Web.UI.WebControls;
using System.IO;
using DataTable = PIP.Models.DataTable;

namespace PIP.Controllers
{
    
    public class HomeController : Controller
    {
        // GET: Home

        public bool Tokenvalidation()
        {
            try
            {
                if (Session["username"].ToString() == Authentication.ValidateToken(Session["jwtSecurityToken"].ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Response.Write("Token Expired");
                return false;
            }
        }
       
        public ActionResult Login()
        {
            if (Session["username"]!=null)
            { 
                Session.Clear();                
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]        
        public ActionResult Login(RegistrationTable registration)
        {
            try
            {
                using (Database1Entities19 RegistrationEntity = new Database1Entities19())
                {
                    if (ModelState.IsValid)
                    {
                        var log = RegistrationEntity.RegistrationTables.Where(a => a.UserName.Equals(registration.UserName) && a.Password.Equals(registration.Password)).FirstOrDefault();
                        if (log != null)
                        {
                            var jwtSecurityToken = Authentication.GenereateJwtToken(log.UserName);
                            Session["username"] = log.UserName;
                            Session["fn"] = log.FirstName;
                            Session["ln"] = log.LastName;
                            var validUserName = Authentication.ValidateToken(jwtSecurityToken);
                            Session["jwtSecurityToken"] = jwtSecurityToken;
                            if (validUserName == log.UserName)
                            {
                                return RedirectToAction("HomePage", "Home", new { token = jwtSecurityToken });
                            }
                            else
                            {
                                //Response.Write("<scipt> Token Validation Failed</script>");
                                ViewBag.alert = "Token Validation Failed";
                                return View();
                            }

                        }
                        else
                        {
                            //Response.Write("<scipt> Invalid Credentials</script>");
                            ViewBag.alert = "Invalid Credentials";
                            ModelState.Clear();
                        }
                    }
                }
                return View();
            }
            catch
            {
                return View("Error");
            }
        }
        
        public ActionResult RegistrationPage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RegistrationPage(RegistrationTable registration)
        {
            try
            {
                using (Database1Entities19 RegistrationEntity = new Database1Entities19())
                {
                    if (registration.Password == null || registration.UserName == null || registration.LastName == null || registration.FirstName == null || registration.Address == null || registration.Email == null)
                    {
                        //Response.Write("<scipt>Enter all fields</script>");
                        ViewBag.alert = "Enter all fields";
                        return View();
                    }
                    RegistrationTable duplicateUseraname = RegistrationEntity.RegistrationTables.Where(x => x.UserName == registration.UserName).FirstOrDefault();
                    RegistrationTable duplicateEmail = RegistrationEntity.RegistrationTables.Where(x => x.Email == registration.Email).FirstOrDefault();
                    if (duplicateUseraname != null)
                    {
                        ViewBag.alert = "Username Taken";
                        ModelState.Clear();
                        return View(duplicateUseraname);
                    }
                    if (duplicateEmail != null)
                    {
                        ViewBag.alert = "Email in existence";
                        ModelState.Clear();
                        return View(duplicateEmail);
                    }

                    if (ModelState.IsValid)
                    {
                        RegistrationEntity.RegistrationTables.Add(registration);
                        RegistrationEntity.SaveChanges();
                        LikeTableUpdate(registration);
                        ViewBag.alert = "User Registered";
                        ModelState.Clear();
                        // Response.Write("<scipt> User Registered</script>");

                    }
                }
                return View();
            }
            catch
            {
                return View("Error");
            }
        }             
        
        public void LikeTableUpdate(string id)
        {
            Database1Entities22 LikeEntity = new Database1Entities22();
            Database1Entities19 RegistrationEntity = new Database1Entities19();            
            List<RegistrationTable> registrations = RegistrationEntity.RegistrationTables.ToList();
            for(int i=0;i<registrations.Count;++i)
            {
                LikeTable likeTable = new LikeTable();
                List<LikeTable> likeTables = LikeEntity.LikeTables.ToList();
                likeTable.Username = registrations[i].UserName.ToString();
                likeTable.CommentId = id;
                likeTable.like = 0;
                likeTable.id = (likeTables.Count)+1;
                LikeEntity.LikeTables.Add(likeTable);
                LikeEntity.SaveChanges();
            }

        }
        public void LikeTableUpdate(RegistrationTable registration)
        {
            Database1Entities22 LikeEntity = new Database1Entities22();
            Database1Entities21 entities1 = new Database1Entities21();
            List<CommentTable> CommentList = entities1.CommentTables.ToList();
            for (int i = 0; i < CommentList.Count; ++i)
            {
                LikeTable likeTable = new LikeTable();
                List<LikeTable> likeTables = LikeEntity.LikeTables.ToList();
                likeTable.Username = registration.UserName.ToString();
                likeTable.CommentId = CommentList[i].Id;
                likeTable.like = 0;
                likeTable.id = (likeTables.Count) + 1;
                LikeEntity.LikeTables.Add(likeTable);
                LikeEntity.SaveChanges();
            }
        }        
        public BlogViewModel AssignBlogData()
        {
            Database1Entities21 CommentEntity = new Database1Entities21();
            Database1Entities17 BlogEntity = new Database1Entities17();
            List<BlogTable> BlogList = BlogEntity.BlogTables.ToList();
            List<CommentTable> CommentList = CommentEntity.CommentTables.ToList();
            CommentTable newComment = new CommentTable();
            BlogViewModel mymodel = new BlogViewModel();
            mymodel.PreviousComment = CommentList;
            mymodel.NewComment = newComment;
            mymodel.BlogList = BlogList;
            if (Session["blogid"] != null)
            {
                mymodel.blogid = (int)Session["blogid"];
            }
            return (mymodel);

        }

        public ActionResult Comment()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Home");
            }           
            return RedirectToAction("blog");
        }
       
        [HttpPost]
        public ActionResult Comment(BlogViewModel text)
        {
            try
            {
                using (Database1Entities21 CommentEntity = new Database1Entities21())
                {

                    if (ModelState.IsValid)
                    {
                        List<CommentTable> CommentList = CommentEntity.CommentTables.ToList();
                        Database1Entities17 BlogEntity = new Database1Entities17();
                        List<BlogTable> BlogList = BlogEntity.BlogTables.ToList();
                        text.PreviousComment = CommentList;
                        if (text.NewComment.Comment == null)
                        {
                            //Response.Write("<scipt>Invalid Comment</script>");
                            TempData["alert"] = "Invalid Comment";
                            return RedirectToAction("blog");
                        }
                        text.BlogList = BlogList;
                        text.NewComment.Username = Session["username"].ToString();
                        text.NewComment.Like = 0;
                        text.NewComment.Unlike = 0;
                        string blogid = Session["blogid"].ToString();
                        text.NewComment.blogid = Int32.Parse(blogid);
                        text.blogid = text.NewComment.blogid;
                        if (CommentList.Count == 0)
                        {
                            text.NewComment.Id = "1";
                        }
                        else
                        {
                            int max = 1;

                            for (int i = 0; i < CommentList.Count; ++i)
                            {
                                string[] CommentIdBreak = CommentList[i].Id.Split('.');
                                if (Int32.Parse(CommentIdBreak[0]) > max )
                                {
                                    max = Int32.Parse(CommentIdBreak[0]);
                                }
                            }
                            text.NewComment.Id = (max + 1).ToString();
                        }                        
                        LikeTableUpdate(text.NewComment.Id);
                        CommentEntity.CommentTables.Add(text.NewComment);
                        CommentEntity.SaveChanges();
                        ModelState.Clear();
                    }


                }
                ModelState.Clear();
                return RedirectToAction("Blog");
        }
            catch
            {
                return View("error");
            }


        }
        public ActionResult NestedComment()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return RedirectToAction("blog");
        }
        [HttpPost]
        public ActionResult NestedComment(BlogViewModel text)
        {
            try
            {
                if (text.NewComment.Comment == null)
                {
                    //Response.Write("<scipt>Invalid Comment</script>");
                    TempData["alert"] = "Invalid Comment";
                    return RedirectToAction("blog");
                }
                using (Database1Entities21 CommentEntity = new Database1Entities21())
                {                    
                    Database1Entities17 BlogEntity = new Database1Entities17();
                    text.BlogList = BlogEntity.BlogTables.ToList();
                    List<CommentTable> commentlist = CommentEntity.CommentTables.ToList();
                    text.PreviousComment = commentlist;
                    text.NewComment.Username = Session["username"].ToString();
                    text.NewComment.Like = 0;
                    text.NewComment.Unlike = 0;
                    string blogid = Session["blogid"].ToString();
                    text.NewComment.blogid = Int32.Parse(blogid);
                    text.blogid = text.NewComment.blogid;

                    
                    int IdCount = 0;
                    for(int i=0;i<commentlist.Count;++i)
                    {
                        bool Subid=true;
                        if (commentlist[i].Id.Count(x => (x == '.')) == text.NewComment.Id.Count(x => (x == '.'))+1)
                        {
                        for (int j = 0; j < text.NewComment.Id.Length; ++j)
                        {
                            if (commentlist[i].Id[j] != text.NewComment.Id[j])
                            {
                                Subid = false;break;
                            }                            
                        }
                        if (Subid )
                        {
                            ++IdCount;
                        }

                        }
                        
                    }
                    ++IdCount;
                    text.NewComment.Id = text.NewComment.Id + "." + IdCount.ToString();                    
                    LikeTableUpdate(text.NewComment.Id);
                    CommentEntity.CommentTables.Add(text.NewComment);
                    CommentEntity.SaveChanges();
                    ModelState.Clear();
                }

                return RedirectToAction("Blog");
        }
            catch
            {
                return View("error");
            }
        }
        public ActionResult Like()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return RedirectToAction("blog");
        }
        [HttpPost]
        public ActionResult Like(string button,string likeid)
        {
            try
            {
                using (Database1Entities21 CommentEntity = new Database1Entities21())
                {
                    Database1Entities22 LikeEntity = new Database1Entities22();
                    CommentTable f = CommentEntity.CommentTables.FirstOrDefault(x => x.Id == likeid);
                    string CurrentUser = Session["username"].ToString();
                    LikeTable likes = LikeEntity.LikeTables.Where(x => x.CommentId == likeid && x.Username.Equals(CurrentUser)).FirstOrDefault();
                    if (likes.like == 0)
                    {
                        if (button == "like")
                        {
                            ++f.Like; likes.like = 1;
                        }

                        else
                        {
                            ++f.Unlike; likes.like = -1;
                        }
                        CommentEntity.SaveChanges();
                        LikeEntity.SaveChanges();
                    }
                    else if (likes.like == -1 && button == "like")
                    {

                        --f.Unlike; likes.like = 1; ++f.Like;
                        CommentEntity.SaveChanges();
                        LikeEntity.SaveChanges();

                    }
                    else if (likes.like == 1 && button == "unlike")
                    {

                        --f.Like; likes.like = -1; ++f.Unlike;
                        CommentEntity.SaveChanges();
                        LikeEntity.SaveChanges();

                    }
                    return RedirectToAction("blog");
                }
            }
            catch
            {
                return View("error");
            }
        } 
        //public ActionResult Blogidsetter(int id)
        //{
        //    Session["blogid"] = id;            
        //    return View("blog",AssignBlogData());
        //}
        
        public ActionResult Blog()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Home");
            }                  
            return View(AssignBlogData());
        }
        [HttpPost]
        public ActionResult blog(BlogViewModel id)
        {
            string blogid = Convert.ToString(Request["choice"]);
            if(blogid == null)
            {
                return View(AssignBlogData());
            }
            int blogid1 = Int32.Parse(blogid);
            Session["blogid"] = blogid1;
            return View(AssignBlogData());
        }

            //    Database1Entities16 CommentEntity = new Database1Entities16();
            //    Database1Entities17 BlogEntity = new Database1Entities17();
            //    List<BlogTable> BlogList = BlogEntity.BlogTables.ToList();
            //    List<CommentTable> CommentList = CommentEntity.CommentTables.ToList();
            //    CommentTable newComment = new CommentTable();
            //    BlogViewModel mymodel = new BlogViewModel();
            //    mymodel.PreviousComment = CommentList;
            //    mymodel.NewComment = newComment;
            //    mymodel.BlogList = BlogList;
            //    mymodel.blogid = id;
            //    Session["blogid"] = id;
            //    return View("blog",mymodel);

            // }
        public ActionResult AddBlog()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("login", "home");
            }
            Session["blogid"] = null;
            return View();
        }

        [HttpPost]
        public ActionResult AddBlog(BlogTable blogcontent)
        {
            try
            {
                using (Database1Entities17 BlogEntity = new Database1Entities17())
                {
                    if (ModelState.IsValid)
                    {
                        List<BlogTable> bloglist = BlogEntity.BlogTables.ToList();
                        if (blogcontent.content == null)
                        {
                            //Response.Write("<scipt>Invalid Content</script>");
                            ViewBag.alert = "Invalid Content";
                        }
                        else if (blogcontent.Name == null)
                        {
                            //Response.Write("<scipt>Invalid Name</script>");
                            ViewBag.alert = "Invalid Name";
                        }
                        else
                        {
                            int max = 0;
                            for (int i = 0; i < bloglist.Count; i++)
                            {
                                if (bloglist[i].Id > max)
                                {
                                    max = bloglist[i].Id;
                                }
                            }
                            blogcontent.Id = max + 1;
                            BlogEntity.BlogTables.Add(blogcontent);
                            BlogEntity.SaveChanges();
                            ModelState.Clear();
                            return RedirectToAction("Blog", "Home");
                        }
                    }
                }
                return View(blogcontent);
            }
            catch
            {
                return View("error");
            }
        }
        public bool CheckPrevRecords(DataTable newFile)
        {
            Database1Entities20 DataEntity = new Database1Entities20();
           // List<DataTable> filelist = DataEntity.DataTables.ToList();
            DataTable duplicate = DataEntity.DataTables.Where(x => x.data1 == newFile.data1 && x.data2.Equals(newFile.data2)&&x.data3==newFile.data3).FirstOrDefault();
            if (duplicate == null)
                return true;
            else
                return false;
        }
       
        public ActionResult HomePage()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("login", "home");
            }            
            return View();
        }

        [HttpPost]
        public ActionResult HomePage(FileModel file)
        {
            try
            {
                if (Tokenvalidation())
                {
                    string extension = Path.GetExtension(file.UploadedFile.FileName);
                    if (extension == ".csv")
                    {
                        StreamReader csvreader = new StreamReader(file.UploadedFile.InputStream);
                        Database1Entities20 DataEntity = new Database1Entities20();
                        int RecordCount = 0, TotalFileRecordCount = 0;
                        if (file.UploadedFile.ContentLength > 0)
                        {
                            while (!csvreader.EndOfStream)
                            {
                                List<DataTable> filelist = DataEntity.DataTables.ToList();
                                DataTable newfile = new DataTable();
                                var line = csvreader.ReadLine(); ++TotalFileRecordCount;
                                string[] values = line.Split(';');
                                newfile.data1 = values[0];
                                newfile.data2 = values[1];
                                newfile.data3 = values[2];
                                if (CheckPrevRecords(newfile))
                                {
                                    newfile.modifiedby = Session["username"].ToString();
                                    newfile.id = filelist.Count + 1;
                                    DataEntity.DataTables.Add(newfile);
                                    DataEntity.SaveChanges();
                                }
                                else
                                {
                                    ++RecordCount;
                                    //ViewBag.alert = "Duplicate Record Detected";
                                }
                            }
                            //Response.Write("<scipt>File Uploaded</script>");
                            if (RecordCount == 1)
                            {
                                //Response.Write("<script>Duplicate Record Detected</script>");
                                ViewBag.alert = "File Uploaded,Duplicate Record Detected";
                            }
                            else if (RecordCount > 1 && RecordCount < TotalFileRecordCount)
                            {
                                ViewBag.alert = "File Uploaded, " + RecordCount + " Duplicate Records Detected";
                            }
                            else if (RecordCount == 0)
                            {
                                ViewBag.alert = "File Uploaded";
                            }
                            else
                            {
                                ViewBag.alert = "File Not uploaded";
                            }
                        }
                    }
                    else
                    {
                        // Response.Write("<scipt>Only CSV files allowed</script>");
                        ViewBag.alert = "Only CSV files allowed";
                    }
                    return View();
                }
                else
                {
                    //Response.Write("<scipt>Invalid Token</script>");
                    Session.Clear();
                    ViewBag.alert = "Invalid Token";                    
                    return View("Login");
                }
            }
            catch
            {
                return View("error");
            }
            
        }
        public ActionResult DataTable()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("login", "home");
            }
            Database1Entities20 DataEntity = new Database1Entities20();
            List<DataTable> dataTables = DataEntity.DataTables.ToList();
            DataTableModel dataTableModel = new DataTableModel();
            dataTableModel.allData = DataEntity.DataTables.ToList();
            dataTableModel.pageNo = 1;
            dataTableModel.pageCount=(dataTables.Count/15);
            if (dataTables.Count % 15 > 0)
                dataTableModel.pageCount++;
            return View(dataTableModel);
        }
        [HttpPost]
        public ActionResult DataTable(DataTableModel dataTableModel)
        {
            try
            {
                string Pageno = Convert.ToString(Request["page"]);
                Database1Entities20 DataEntity = new Database1Entities20();
                List<DataTable> dataTables = DataEntity.DataTables.ToList();
                dataTableModel.allData = DataEntity.DataTables.ToList();
                dataTableModel.pageNo = Int32.Parse(Pageno);
                dataTableModel.pageCount = (dataTables.Count / 15);
                if (dataTables.Count % 15 > 0)
                    dataTableModel.pageCount++;
                return View(dataTableModel);
            }
            catch
            {
                return View("error");
            }
        }
        //public ActionResult Error()
        //{
        //    //if (Session["username"] == null)
        //    //{
        //    //    return RedirectToAction("Login", "Home");
        //    //}
        //    return View();
        //}

        [HttpPost]
        public ActionResult ErrorComplaints(ComplaintsTable complaints)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Database1Entities23 ErrorEntity = new Database1Entities23();
                    ErrorEntity.ComplaintsTables.Add(complaints);
                    ErrorEntity.SaveChanges();
                    TempData["message"] = "Error Complaint Registered";
                    ModelState.Clear();
                }
            }
            catch
            {
                TempData["message"] = "Error Complaint has already been registered";                
            }
            return RedirectToAction("homepage");
        }

    }
}