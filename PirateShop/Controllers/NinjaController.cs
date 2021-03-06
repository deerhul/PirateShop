﻿using Microsoft.AspNet.Identity;
using PirateShop.Models;
using PirateShop.Models.Items;
using PirateShop.Models.Methods;
using PirateShop.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;

namespace PirateShop.Controllers
{
    public class NinjaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Utilities util;
        private PictureMethod picMethod;

        public NinjaController()
        {
            //Research application injection
            _context = new ApplicationDbContext();
            util = new Utilities(_context);

        }

        // GET: Pirate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NinjaDisplay()
        {
            List<NinjaClanViewModel> NCV = util.CreateNinjaDisplay();

            return View(NCV);

        }

        [Authorize]
        public ActionResult Create()
        {
            NinjaViewModel vm = util.makeModel(null);
            //AlertMsg("Created Ninja", "test");
            if (!ModelState.IsValid)
            {
                vm = util.makeModel(null);
            }
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(NinjaViewModel viewModel)
        {
            try
            {
                picMethod = new PictureMethod(_context);

                // the id of logged-in member
                var creatorId = User.Identity.GetUserId();
                var clanSelected = viewModel.Clan.ClanID;
                int genderSelected = viewModel.Gender.ID;

                //youtube vid link for imageupload section: https://www.youtube.com/watch?v=5L5W-AE-sEs
                string filename
                    = Path.GetFileNameWithoutExtension(viewModel.ImageFile.FileName);
                string extension
                    = Path.GetExtension(viewModel.ImageFile.FileName);
                string filename2DB;

                //save image name is unique to allow duplicate names (time/date inclusive in name)
                filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                filename2DB = filename; // for saving
                filename = Path.Combine(Server.MapPath("~/Images/"), filename);
                picMethod.SaveImage(viewModel, filename);

                var creator = _context.Users.Single(u => u.Id == creatorId);

                Ninja ninja = null;

                if (!filename.IsEmpty())
                {
                    ninja = new Ninja
                    {
                        Name = viewModel.Name,
                        clanID = clanSelected,
                        genderID = genderSelected,
                        Age = viewModel.Age,
                        Creator = creator,
                        NinjaImage = "~/Images/" + filename2DB
                    };
                }
                else
                {
                    ninja = new Ninja
                    {
                        Name = viewModel.Name,
                        clanID = clanSelected,
                        genderID = genderSelected,
                        Age = viewModel.Age,
                        Creator = creator,
                    };
                }



                _context.Clans.Single(c => c.ClanID == ninja.clanID).Members++;

                //save changes to database
                _context.Ninjas.Add(ninja);
                _context.SaveChanges();



            }
            catch (Exception e)
            {
                util.AlertMsg("Create ninja", "Failed: " + e.ToString());
                viewModel = util.makeModel("Repopulating NinjaViewModel...");
                util.AlertMsg("Message: ", viewModel.Message2View);
                return View(viewModel);
            }

            //return RedirectToAction("Index", "Home");
            //not currently working
            string msg = string.Format("{0}, {1}, {2}, from the {3}",
                viewModel.Name,
                viewModel.Age,
                viewModel.Gender.gender,
                viewModel.Clan.ClanName);
            util.AlertMsg("Created Ninja...", msg);
            return View("NinjaDisplay", util.CreateNinjaDisplay());
        }

        public ActionResult Delete(int id)
        {
            if (Session["user"] != null) //prevent not-logged in users from having access to delete method
            {
                try
                {
                    Ninja temp = null;

                    foreach (Ninja item in _context.Ninjas)
                    {
                        if (item.ID.Equals(id))
                        {
                            temp = item;
                            break;
                        }
                    }

                    if (temp != null)
                    {
                        _context.Ninjas.Remove(temp);
                        _context.SaveChanges();
                        util.AlertMsg("Delete ninja", "Ninja deletion success");
                    }
                }
                catch (Exception e)
                {
                    util.AlertMsg(null, e.ToString());
                }
            }

            return View("NinjaDisplay", util.CreateNinjaDisplay());
        }
    }
}