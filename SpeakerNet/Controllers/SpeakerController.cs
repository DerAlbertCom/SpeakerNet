using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpeakerNet.Extensions;
using SpeakerNet.FilterAttributes;
using SpeakerNet.Models;
using SpeakerNet.Services;
using SpeakerNet.ViewModels;

namespace SpeakerNet.Controllers
{
    public class SpeakerController : SpeakerNetController
    {
        readonly ISpeakerService speakerService;
        readonly ISendMailService mailService;

        public SpeakerController(ISpeakerService speakerService, ISendMailService mailService)
        {
            this.speakerService = speakerService;
            this.mailService = mailService;
        }

        public ActionResult Help(Guid id)
        {
            return View(new SpeakerHelpModel {Id = id});
        }

        public ActionResult Details(Guid id)
        {
            return View(speakerService.GetSpeaker(id).MapFrom<Speaker, SpeakerEditModel>());
        }

        public ActionResult Edit(Guid id)
        {
            return View(speakerService.GetSpeaker(id).MapFrom<Speaker, SpeakerEditModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditCommand(Guid id, SpeakerEditModel model)
        {
            if (ModelState.IsValid) {
                speakerService.UpdateSpeaker(id, model);
                return RedirectToAction("Details", new {id});
            }
            return View(model);
        }


        [AdminOnly]
        public ActionResult Create()
        {
            return View(new CreateSpeakerModel());
        }

        [AdminOnly]
        public ActionResult SendMail(Guid id)
        {
            var sendMailModel = speakerService.GetSpeaker(id).MapFrom<Speaker, SpeakerSendMailModel>();
            AddTemplates(sendMailModel);
            return View(sendMailModel);
        }

        [AdminOnly]
        public ActionResult SendMailAll()
        {
            var model = new SpeakerSendMailAllModel();
            AddTemplates(model);
            return View(model);
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult SendMailAll(SpeakerSendMailAllModel model)
        {
            if (ModelState.IsValid)
            {
                mailService.SendMailToAll(model.TemplateId, Request, Url);
                return RedirectToAction("List");
            }
            AddTemplates(model);
            return View(model);
        }

        [AdminOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMail(Guid id, SpeakerSendMailModel model)
        {
            var speaker = speakerService.GetSpeaker(id);

            if (string.IsNullOrWhiteSpace(speaker.Contact.EMail)) {
                ModelState.AddModelError("", "F�r den Sprecher ist keine Mail hinterlegt");
            }

            if (ModelState.IsValid) {
                try {
                    mailService.SendMail(speaker.Contact.EMail, model.Subject, model.Body);
                    return RedirectToAction("Details", new {id});
                }
                catch (Exception e) {
                    ModelState.AddModelError("", string.Format("{0}: {1} ", e.GetType().Name, e.Message));
                }
            }
            var sendMailModel = speakerService.GetSpeaker(id).MapFrom<Speaker, SpeakerSendMailModel>();
            AddTemplates(sendMailModel);
            return View(sendMailModel);
        }

        [HttpPost]
        [AdminOnly]
        public ActionResult GetTemplate(Guid speakerId, Guid templateId)
        {
            if (!Request.IsAjaxRequest())
                return new HttpNotFoundResult();

            var speaker = speakerService.GetSpeaker(speakerId);
            var template = mailService.GetTemplate(templateId);

            var model = new {
                speaker.FirstName,
                speaker.LastName,
                SpeakerUrl = mailService.GetSpeakerUrl(speaker.Id, Request, Url)
            };

            return Json(new {
                Subject = template.Subject.NamedFormat(model),
                Body = template.Body.NamedFormat(model)
            });
        }

        void AddTemplates(SpeakerSendMailAllModel sendMailModel)
        {
            var mailTemplates = mailService.GetMailTemplates();
            sendMailModel.TemplateList = new SelectList(mailTemplates, "Id", "Description");
            if (mailTemplates.Any())
            {
                sendMailModel.TemplateId = mailTemplates.First().Id;
            }
        }

        void AddTemplates(SpeakerSendMailModel sendMailModel)
        {
            var mailTemplates = mailService.GetMailTemplates();
            if (mailTemplates.Any()) {
                var mt = mailTemplates.First();
                sendMailModel.TemplateList = new SelectList(mailTemplates, "Id", "Description");
                var model = new {
                    sendMailModel.FirstName,
                    sendMailModel.LastName,
                    SpeakerUrl = mailService.GetSpeakerUrl(sendMailModel.Id, Request, Url)
                };
                sendMailModel.Subject = mt.Subject.NamedFormat(model);
                sendMailModel.Body = mt.Body.NamedFormat(model);
            }
        }

        [AdminOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateSpeakerModel model)
        {
            if (ModelState.IsValid) {
                if (speakerService.CreateSpeaker(model))
                    return RedirectToAction("List");
            }
            return View();
        }

        [AdminOnly]
        public ActionResult List()
        {
            return View(speakerService.GetSpeakerList());
        }
    }
}