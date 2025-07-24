using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class MediaOperations : GenericRepository
    {
        public MediaOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public SermonViewModel GetDashboard(string churchId)
        {
            return new SermonViewModel()
            {
                SeriesList = Db.SermonSeries.Where(x => x.ChurchId == churchId).OrderByDescending(x => x.CreatedDate).ToList() ?? new List<SermonSeries>(),
                TopicsList = Db.SermonTopics.ToList() ?? new List<SermonTopic>()
            };
        }

        public SermonView SermonDashboard(string sermonId)
        {
            var vm = new SermonView
            {
                Sermon = Db.Sermons.Find(sermonId)
            };
            vm.Series = vm.Sermon != null ? Db.SermonSeries.Find(vm.Sermon.SeriesId) : new SermonSeries();
            vm.Topic = vm.Sermon != null ? Db.SermonTopics.Find(vm.Sermon.TopicId) : new SermonTopic();
            vm.Notes = vm.Sermon != null ? Db.SermonNotes.Find(vm.Sermon.Id) : new SermonNote();
            return vm;
        }

        public SeriesView GetSeriesView(string churchId, string seriesId)
        {
            var model = new SeriesView();

            if (string.IsNullOrEmpty(seriesId))
            {
                return model;
            }

            model.Series = Db.SermonSeries.FirstOrDefault(x => x.ChurchId == churchId && x.Id == seriesId);
            if (model.Series != null)
            {
                model.Topic = Db.SermonTopics.Find(model.Series.TopicId);
                model.Sermons = Db.Sermons.Where(x => x.SeriesId == seriesId).ToList();
            }

            model.TopicsList = Db.SermonTopics.ToList();
            return model;
        }

        public List<SermonNotesListViewModel> GetSermonNotesListViewModel(string sermonId)
        {
            return (from note in Db.SermonNotes
                    from sermon in Db.Sermons
                    where note.SermonId == sermonId && sermon.Id == sermonId && note.NoteType == "Standard"
                    select new SermonNotesListViewModel
                    {
                        NoteID = note.Id,
                        SermonTitle = sermon.Title,
                        SermonID = sermon.Id,
                        CreatedDate = note.CreatedDate
                    }).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<SermonNotesListViewModel> GetSermonBlankNotesList(string sermonId)
        {
            return (from note in Db.SermonNotes
                    from sermon in Db.Sermons
                    where note.SermonId == sermonId && sermon.Id == sermonId && note.NoteType == "Blank"
                    select new SermonNotesListViewModel
                    {
                        NoteID = note.Id,
                        SermonTitle = sermon.Title,
                        SermonID = sermon.Id
                    }).ToList();
        }

        public List<SermonNotesListViewModel> GetSermonFilledNotesList(string sermonId)
        {
            return (from note in Db.SermonNotes
                    from sermon in Db.Sermons
                    where note.SermonId == sermonId && sermon.Id == sermonId && note.NoteType == "Filled"
                    select new SermonNotesListViewModel
                    {
                        NoteID = note.Id,
                        SermonTitle = sermon.Title,
                        SermonID = sermon.Id,
                        CreatedDate = note.CreatedDate
                    }).OrderByDescending(x => x.CreatedDate).ToList();
        }

        #region Sermon Series
        public SermonSeries GetSermonSeries(string id)
        {
            return Read<SermonSeries>().FirstOrDefault(x => x.Id == id);
        }

        public List<SermonSeries> GetAllSermonSeries(string churchId)
        {
            return Read<SermonSeries>().Where(x => x.ChurchId == churchId).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public void CreateSermonSeries(SermonSeries entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateSermonSeries(SermonSeries sermonSeries)
        {
            Update(sermonSeries);
            SaveChanges();
        }

        public void DeleteSeries(string id)
        {
            var entity = GetSermonSeries(id);
            Delete(entity);
            SaveChanges();
        }
        #endregion

        #region Sermon
        public Sermon GetSermon(string id)
        {
            return Read<Sermon>().FirstOrDefault(x => x.Id == id);
        }

        public List<Sermon> GetAllSermon(string churchId)
        {
            return Read<Sermon>().Where(x => x.ChurchId == churchId).ToList();
        }

        public void CreateSermon(Sermon sermon)
        {
            Create(sermon);
            SaveChanges();
        }

        public void UpdateSermon(Sermon entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteSermon(string id)
        {
            var entity = GetSermon(id);
            DeleteSermon(entity);
        }

        public void DeleteSermon(Sermon entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion

        #region Sermon Notes
        public List<SermonNote> GetAllSermonNote()
        {
            return Read<SermonNote>().ToList();
        }

        public SermonNote GetSermonNote(string id)
        {
            return Read<SermonNote>().FirstOrDefault(x => x.Id == id);
        }

        public void CreateSermonNote(SermonNote entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateSermonNote(SermonNote entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteSermonNote(string id)
        {
            var entity = GetSermonNote(id);
            DeleteSermonNote(entity);
        }

        public void DeleteSermonNote(SermonNote entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion

        #region Sermon Topic
        public List<SermonTopic> GetAllSermonTopic()
        {
            return Read<SermonTopic>().OrderBy(x => x.Title).ToList();
        }

        public SermonTopic GetSermonTopic(string id)
        {
            return Read<SermonTopic>().FirstOrDefault(x => x.Id == id);
        }

        public void CreateSermonTopic(SermonTopic entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void DeleteSermonTopic(string id)
        {
            var entity = GetSermonTopic(id);
            DeleteSermonTopic(entity);
        }

        public void UpdateSermonTopic(SermonTopic entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteSermonTopic(SermonTopic entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion
    }
}