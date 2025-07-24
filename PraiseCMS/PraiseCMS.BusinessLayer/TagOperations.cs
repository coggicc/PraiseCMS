using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class TagOperations : GenericRepository
    {
        public TagOperations(ApplicationDbContext db, Work work) : base(db, work) { }

        public Tag Get(string id)
        {
            return Read<Tag>().FirstOrDefault(x => x.Id == id);
        }

        public Folder GetFolder(string id)
        {
            return Read<Folder>().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Tag> Get(IEnumerable<string> ids)
        {
            return Read<Tag>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public IEnumerable<Tag> GetTagsByFolders(IEnumerable<string> ids)
        {
            return Read<Tag>().Where(x => ids.Contains(x.FolderId)).ToList();
        }

        public IEnumerable<Tag> GetTagsByFolder(string id)
        {
            return Read<Tag>().Where(x => x.FolderId.Equals(id)).ToList();
        }

        public IEnumerable<Folder> GetFolders(IEnumerable<string> ids)
        {
            return Read<Folder>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public Result<Tag> Create(Tag entity)
        {
            try
            {
                Create<Tag>(entity);
                SaveChanges();
                return new Result<Tag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Tag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Folder> Create(Folder entity)
        {
            try
            {
                Create<Folder>(entity);
                SaveChanges();
                return new Result<Folder>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Folder>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Tag> Delete(Tag entity)
        {
            try
            {
                //delete people assign to this tag
                var tagPeople = GetTagPeople(entity.Id);
                Delete<TagPerson>(tagPeople);
                Delete<Tag>(entity);
                SaveChanges();
                return new Result<Tag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Tag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        internal ResponseModel DeleteFolder(IEnumerable<Folder> folders)
        {
            try
            {
                var ids = folders.Select(x => x.Id);
                var childfolders = Read<Folder>().Where(q => !string.IsNullOrEmpty(q.ParentId) && ids.Contains(q.ParentId)).ToList();
                var childTags = GetTagsByFolders(ids);
                var tagPeople = GetTagPeople(childTags.Select(q => q.Id));
                Delete<TagPerson>(tagPeople);
                Delete(childTags);
                if (childfolders.Any())
                {
                    DeleteFolder(childfolders);
                }
                Delete(folders);
                return new ResponseModel() { Success = true };
            }
            catch (Exception ex)
            {
                return new ResponseModel() { Success = false, Message = ex.Message };
            }
        }

        public Result<Folder> Delete(Folder entity)
        {
            try
            {
                var childfolders = Read<Folder>().Where(q => !string.IsNullOrEmpty(q.ParentId) && entity.Id.Equals(q.ParentId)).ToList();
                if (childfolders.Any())
                {
                    DeleteFolder(childfolders);
                }
                var childTags = GetTagsByFolder(entity.Id);
                var tagPeople = GetTagPeople(childTags.Select(q => q.Id));
                Delete<TagPerson>(tagPeople);
                Delete(childTags);
                Delete<Folder>(entity);
                SaveChanges();
                return new Result<Folder>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Folder>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Tag> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                //delete people assign to this tag
                var tagPeople = GetTagPeople(id);
                Delete<TagPerson>(tagPeople);
                Delete<Tag>(entity);
                SaveChanges();
                return new Result<Tag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Tag>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Tag> Update(Tag entity)
        {
            try
            {
                Update<Tag>(entity);
                SaveChanges();
                return new Result<Tag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Tag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Folder> Update(Folder entity)
        {
            try
            {
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                entity.ModifiedDate = DateTime.Now;
                Update<Folder>(entity);
                SaveChanges();
                return new Result<Folder>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Folder>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public List<Tag> GetAll(IEnumerable<string> Ids)
        {
            return Read<Tag>().Where(x => Ids.Contains(x.Id)).OrderBy(x => x.Name).ToList();
        }

        public List<Tag> GetAll(string churchId)
        {
            return Read<Tag>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }

        public List<Folder> GetAllFolders(string churchId)
        {
            return Read<Folder>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }

        public FoldersTagsViewModel GetChildFoldersAndTagsByParentId(string folderId)
        {
            return new FoldersTagsViewModel()
            {
                ParentFolder = Read<Folder>().FirstOrDefault(q => q.Id.Equals(folderId)),
                Folders = Read<Folder>().Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !string.IsNullOrEmpty(x.ParentId) && x.ParentId.Equals(folderId)).OrderBy(x => x.Name).ToList(),
                Tags = Read<Tag>().Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !string.IsNullOrEmpty(x.FolderId) && x.FolderId.Equals(folderId)).OrderBy(x => x.Name).ToList(),
            };
        }

        //pass tags if need to bind tags in json
        public FolderJsonModel BindFoldersHierarchy(List<Folder> folders, List<Tag> tags = null, List<string> selected = null, List<string> opened = null, List<string> disabled = null)
        {
            var jsonFolder = folders.Select(q => new FolderJsonModel()
            {
                Id = q.Id,
                Text = q.Name,
                ParentId = q.ParentId,
                Icon = "fa fa-folder text-warning",
                State = new NodeState()
                {
                    Disabled = disabled.IsNotNullOrEmpty() && disabled.Any() && disabled.Contains(q.Id),
                    Selected = selected.IsNotNullOrEmpty() && selected.Any() && selected.Contains(q.Id),
                    Opened = (opened.IsNotNullOrEmpty() && opened.Any() && opened.Contains(q.Id)) || q.ParentId.IsNullOrEmpty(),
                }
            }).ToList();
            var jsonTag = new List<FolderJsonModel>();

            if (tags.IsNotNullOrEmpty() && tags.Any())
            {
                jsonTag = tags.Select(q => new FolderJsonModel()
                {
                    Id = q.Id,
                    Text = q.Name,
                    ParentId = q.FolderId,
                    Icon = "fas fa-tag text-warning",
                    State = new NodeState()
                    {
                        Disabled = disabled.IsNotNullOrEmpty() && disabled.Any() && disabled.Contains(q.Id),
                        Selected = selected.IsNotNullOrEmpty() && selected.Any() && selected.Contains(q.Id),
                        Opened = opened.IsNotNullOrEmpty() && opened.Any() && opened.Contains(q.Id),
                    }
                }).ToList();
            }
            else
            {
                jsonTag = null;
            }

            var jsonParentFolder = jsonFolder.Find(q => q.ParentId.IsNullOrEmpty());
            return FolderAndTagCascading(jsonParentFolder, jsonFolder, jsonTag);
        }

        internal FolderJsonModel FolderAndTagCascading(FolderJsonModel folders, List<FolderJsonModel> allFolders, List<FolderJsonModel> allTags = null)
        {
            var childfolders = allFolders.Where(q => !string.IsNullOrEmpty(q.ParentId) && q.ParentId.Equals(folders.Id)).ToList();
            var childTags = new List<FolderJsonModel>();

            if (allTags.IsNotNullOrEmpty() && allTags.Any())
            {
                childfolders.AddRange(allTags.Where(q => !string.IsNullOrEmpty(q.ParentId) && q.ParentId.Equals(folders.Id)).ToList());
            }

            foreach (var item in childfolders)
            {
                FolderAndTagCascading(item, allFolders, allTags);
            }

            if (childfolders.Any())
            {
                folders.Children.AddRange(childfolders);
            }

            return folders;
        }

        public List<Folder> CreateDefaultFolders(string churchId)
        {
            var parentFolder = new Folder()
            {
                ChurchId = churchId,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now,
                Id = Utilities.GenerateUniqueId(),
                ParentId = null,
                Name = "All Tags",
                Type = FolderTypes.Tag
            };
            var childFolders = DefaultFolders.Items.Select(q => new Folder
            {
                ChurchId = churchId,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now,
                Id = Utilities.GenerateUniqueId(),
                ParentId = parentFolder.Id,
                Name = q,
                Type = FolderTypes.Tag
            }).ToList();

            childFolders.Add(parentFolder);
            Create<Folder>(childFolders);
            SaveChanges();

            return childFolders.ToList();
        }

        public List<TagPerson> GetTagPeople(string tagId)
        {
            return Read<TagPerson>().Where(x => x.TagId.Equals(tagId)).ToList();
        }

        public List<TagPerson> GetTagPeople(IEnumerable<string> tagIds)
        {
            return Read<TagPerson>().Where(x => tagIds.Contains(x.TagId)).ToList();
        }

        public TagPerson GetTagPerson(string tagId, string personId)
        {
            return Read<TagPerson>().FirstOrDefault(x => x.TagId.Equals(tagId) && x.PersonId.Equals(personId));
        }

        public List<Person> GetPeopleByTag(string tagId)
        {
            var personIds = GetTagPeople(tagId).Select(q => q.PersonId).ToList();
            return Work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id, personIds);
        }

        public Result<TagPerson> AddPeopleToTag(IEnumerable<string> persons, string tagId)
        {
            try
            {
                if (persons.Any() && tagId.IsNotNullOrEmpty())
                {
                    //skip if record already exist
                    var existing = GetTagPeople(tagId).Select(q => q.PersonId);
                    persons = persons.Except(existing);
                    var entities = persons.Select(q => new TagPerson()
                    {
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = q,
                        TagId = tagId
                    }).ToList();
                    Create<TagPerson>(entities);
                    SaveChanges();
                }

                return new Result<TagPerson>
                {
                    List = Read<TagPerson>().Where(x => x.TagId.Equals(tagId)).ToList(),
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TagPerson>
                {
                    Exception = ex,
                    Message = $"{Constants.DefaultErrorMessage} {ex.Message}",
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<bool> AddPeopleToTag(IEnumerable<string> persons, IEnumerable<string> tags)
        {
            try
            {
                if (persons.Any() && tags.Any())
                {
                    var isSaveChange = false;

                    foreach (var tagId in tags)
                    {
                        //skip if record already exist
                        var existing = GetTagPeople(tagId).Select(q => q.PersonId).ToList();
                        persons = persons.Except(existing).ToList();
                        var entities = persons.Select(q => new TagPerson()
                        {
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            Id = Utilities.GenerateUniqueId(),
                            PersonId = q,
                            TagId = tagId
                        }).ToList();

                        if (entities.Any())
                        {
                            Create<TagPerson>(entities);
                            isSaveChange = true;
                        }
                    }

                    if (isSaveChange)
                    {
                        SaveChanges();
                    }
                }

                return new Result<bool>
                {
                    Data = true,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<bool>
                {
                    Exception = ex,
                    Message = $"{Constants.DefaultErrorMessage} {ex.Message}",
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<TagPerson> RemovePeopleFromTag(string personId, string tagId)
        {
            try
            {
                if (personId.IsNotNullOrEmpty() && tagId.IsNotNullOrEmpty())
                {
                    var entity = GetTagPerson(tagId, personId);
                    Delete(entity);
                    SaveChanges();
                    return new Result<TagPerson>
                    {
                        Data = entity,
                        ResultType = ResultType.Success
                    };
                }

                return new Result<TagPerson>
                {
                    Message = $"{Constants.DefaultErrorMessage} PersonId and TagId is required.",
                    ResultType = ResultType.Failure
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TagPerson>
                {
                    Exception = ex,
                    Message = $"{Constants.DefaultErrorMessage} {ex.Message}",
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<TagPerson> RemoveAllPeopleFromTag(string tagId)
        {
            try
            {
                if (tagId.IsNotNullOrEmpty())
                {
                    var entity = GetTagPeople(tagId);
                    Delete<TagPerson>(entity);
                    SaveChanges();

                    return new Result<TagPerson>
                    {
                        ResultType = ResultType.Success
                    };
                }

                return new Result<TagPerson>
                {
                    Message = $"{Constants.DefaultErrorMessage} TagId is required.",
                    ResultType = ResultType.Failure
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TagPerson>
                {
                    Exception = ex,
                    Message = $"{Constants.DefaultErrorMessage} {ex.Message}",
                    ResultType = ResultType.Exception
                };
            }
        }
    }
}