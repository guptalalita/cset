//////////////////////////////// 
// 
//   Copyright 2018 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DataLayerCore.Model;
using System.Text;
using CSETWeb_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CSETWeb_Api.BusinessManagers
{
    /// <summary>
    /// Handles management of documents.
    /// </summary>
    public class DocumentManager
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private CSET_Context db;

        /// <summary>
        /// The current assessment.
        /// </summary>
        private readonly int assessmentId;


        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentManager(int assessmentId)
        {
            this.db = new CSET_Context();
            this.assessmentId = assessmentId;
        }


        /// <summary>
        /// Returns an array of Document instances that are attached to the answer.
        /// </summary>
        /// <returns></returns>
        public List<Document> GetDocumentsForAnswer(int answerId)
        {
            List<Document> list = new List<Document>();

            var files = db.ANSWER.Include("DOCUMENT_FILE").Where(a => a.Answer_Id == answerId).FirstOrDefault()?.DOCUMENT_FILEs().ToList();

            if (files == null)
            {
                return list;
            }

            foreach (var file in files)
            {
                Document doc = new Document()
                {
                    Document_Id = file.Document_Id,
                    Title = file.Title,
                    FileName = file.Name
                };

                list.Add(doc);
            }

            return list;
        }


        /// <summary>
        /// Changes the title of a stored document.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        public void RenameDocument(int id, string title)
        {
            var doc = db.DOCUMENT_FILE.Where(d => d.Document_Id == id).FirstOrDefault();

            // if they provided a file ID we don't know about, do nothing.
            if (doc == null)
            {
                return;
            }

            doc.Title = title;

            db.DOCUMENT_FILE.AddOrUpdate( doc,x=> x.Document_Id);
            db.SaveChanges();
            CSETWeb_Api.BusinessLogic.Helpers.AssessmentUtil.TouchAssessment(doc.Assessment_Id);
        }


        /// <summary>
        /// Deletes a stored document.
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="answerId">The document ID</param>
        public void DeleteDocument(int id, int answerId)
        {
            var doc = db.DOCUMENT_FILE.Where(d => d.Document_Id == id).FirstOrDefault();

            // if they provided a file ID we don't know about, do nothing.
            if (doc == null)
            {
                return;
            }


            // Detach the document from the Answer
            doc.DOCUMENT_ANSWERS.Remove(db.DOCUMENT_ANSWERS.Where(ans => ans.Document_Id == id && ans.Answer_Id == answerId).FirstOrDefault());


            // If we just detached the document from its only Answer, delete the whole document record
            var otherAnswersForThisDoc = db.ANSWER.Where(ans => ans.Assessment_Id == this.assessmentId
                                         && ans.Answer_Id != answerId
                                        && ans.DOCUMENT_FILEs().Select(x => x.Document_Id).Contains(id)).ToList();

            if (otherAnswersForThisDoc.Count == 0)
            {
                db.DOCUMENT_FILE.Remove(doc);
            }

            db.SaveChanges();
            CSETWeb_Api.BusinessLogic.Helpers.AssessmentUtil.TouchAssessment(doc.Assessment_Id);
        }


        /// <summary>
        /// Returns a JSON array indicating which questions have attached the specified document.
        /// </summary>
        /// <param name="id">The document ID</param>
        public List<int> GetQuestionsForDocument(int id)
        {
            var ans = db.DOCUMENT_FILE.Include("ANSWERS").Where(d => d.Document_Id == id).FirstOrDefault().ANSWERs().ToList();

            List<int> qlist = new List<int>();

            foreach (var aaa in ans)
            {
                qlist.Add(aaa.Question_Or_Requirement_Id);
            }

            return qlist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tmpFilename"></param>
        /// <param name="fileHash"></param>
        /// <param name="answerId"></param>
        /// <param name="File_Upload_Id"></param>
        /// <param name="stream_id">only used if moving away from the blob process</param>
        public void AddDocument(string title, string fileName, string contentType, string fileHash, int answerId, byte[] bytes, Guid? stream_id = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "click to edit title";
            }

            // first see if the document already exists on any question in this Assessment, based on the filename and hash
            var doc = db.DOCUMENT_FILE.Where(f => f.FileMd5 == fileHash
                && f.Assessment_Id == this.assessmentId).FirstOrDefault();
            if (doc == null)
            {
                doc = new DOCUMENT_FILE()
                {
                    Assessment_Id = this.assessmentId,
                    Title = title,
                    Path = fileName,  // this may end up being some other reference
                    Name = fileName,
                    FileMd5 = fileHash,
                    ContentType = contentType,
                    Data = bytes
                };

            }
            else
            {
                doc.Title = title;
                doc.Name = fileName;
            }

            var answer = db.ANSWER.Where(a => a.Answer_Id == answerId).FirstOrDefault();
            db.DOCUMENT_FILE.AddOrUpdate( doc, x=> x.Document_Id);
            db.SaveChanges();
            DOCUMENT_ANSWERS temp = new DOCUMENT_ANSWERS() { Answer_Id = answer.Answer_Id, Document_Id = doc.Document_Id }; 
            db.DOCUMENT_ANSWERS.AddOrUpdate( temp,x=> new { x.Document_Id, x.Answer_Id });
            CSETWeb_Api.BusinessLogic.Helpers.AssessmentUtil.TouchAssessment(doc.Assessment_Id);
        }

        /// <summary>
        /// Returns an array of all Document instances that are attached to 
        /// active/relevant answers on the assessment.
        /// </summary>
        /// <returns></returns>
        public List<Document> GetDocumentsForAssessment(int assessmentId)
        {
            List<Document> list = new List<Document>();

            List<int> answerIds = new List<int>();

            // Get the answers for questions or requirements, depending on the application mode
            QuestionsManager qm = new QuestionsManager(assessmentId);
            if (qm.ApplicationMode == "Q")
            {
                answerIds = qm.GetActiveAnswerIds();
            }
            else
            {
                RequirementsManager rm = new RequirementsManager(assessmentId);
                answerIds = rm.GetActiveAnswerIds();
            }


            var dfQuery =
                from df in db.DOCUMENT_FILE
                where (
                    from ans in df.ANSWERs()
                    where answerIds.Contains(ans.Answer_Id)
                    select ans
                ).Any()
                select df;

            var files = dfQuery.ToList();

            if (files == null || files.Count == 0)
            {
                return list;
            }

            foreach (var file in files)
            {
                Document doc = new Document()
                {
                    Document_Id = file.Document_Id,
                    Title = file.Title,
                    FileName = file.Name
                };

                // Don't display "click to edit title" in this context because they won't be able to click it
                if (doc.Title == "click to edit title")
                {
                    doc.Title = "(untitled)";
                }

                list.Add(doc);
            }

            return list;
        }
    }
}

