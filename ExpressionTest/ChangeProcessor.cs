using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class ChangeProcessor
    {
        private CommonDataServices services;
        private DataContext context;
        private ChangeTracker tracker;
        private ChangeDirector changeDirector;
        private ChangeProcessor.EdgeMap currentParentEdges;
        private ChangeProcessor.EdgeMap originalChildEdges;
        private ChangeProcessor.ReferenceMap originalChildReferences;

        internal ChangeProcessor(CommonDataServices services, DataContext context)
        {
            this.services = services;
            this.context = context;
            this.tracker = services.ChangeTracker;
            this.changeDirector = services.ChangeDirector;
            this.currentParentEdges = new ChangeProcessor.EdgeMap();
            this.originalChildEdges = new ChangeProcessor.EdgeMap();
            this.originalChildReferences = new ChangeProcessor.ReferenceMap();
        }

        internal void SubmitChanges(ConflictMode failureMode)
        {
            this.TrackUntrackedObjects();
            this.ApplyInferredDeletions();
            this.BuildEdgeMaps();
            List<TrackedObject> orderedList = this.GetOrderedList();
            ChangeProcessor.ValidateAll((IEnumerable<TrackedObject>)orderedList);
            int totalUpdatesAttempted = 0;
            ChangeConflictSession session = new ChangeConflictSession(this.context);
            List<ObjectChangeConflict> conflictList = new List<ObjectChangeConflict>();
            List<TrackedObject> deletedItems = new List<TrackedObject>();
            List<TrackedObject> insertedItems = new List<TrackedObject>();
            List<TrackedObject> list = new List<TrackedObject>();
            foreach (TrackedObject trackedObject in orderedList)
            {
                try
                {
                    if (trackedObject.IsNew)
                    {
                        if (trackedObject.SynchDependentData())
                            list.Add(trackedObject);
                        this.changeDirector.Insert(trackedObject);
                        insertedItems.Add(trackedObject);
                    }
                    else if (trackedObject.IsDeleted)
                    {
                        ++totalUpdatesAttempted;
                        if (this.changeDirector.Delete(trackedObject) == 0)
                            conflictList.Add(new ObjectChangeConflict(session, trackedObject, false));
                        else
                            deletedItems.Add(trackedObject);
                    }
                    else if (trackedObject.IsPossiblyModified)
                    {
                        if (trackedObject.SynchDependentData())
                            list.Add(trackedObject);
                        if (trackedObject.IsModified)
                        {
                            ChangeProcessor.CheckForInvalidChanges(trackedObject);
                            ++totalUpdatesAttempted;
                            if (this.changeDirector.Update(trackedObject) <= 0)
                                conflictList.Add(new ObjectChangeConflict(session, trackedObject));
                        }
                    }
                }
                catch (ChangeConflictException ex)
                {
                    conflictList.Add(new ObjectChangeConflict(session, trackedObject));
                }
                if (conflictList.Count > 0)
                {
                    if (failureMode == ConflictMode.FailOnFirstConflict)
                        break;
                }
            }
            if (conflictList.Count > 0)
            {
                this.changeDirector.RollbackAutoSync();
                foreach (TrackedObject trackedObject in list)
                    trackedObject.SynchDependentData();
                this.context.ChangeConflicts.Fill(conflictList);
                throw ChangeProcessor.CreateChangeConflictException(totalUpdatesAttempted, conflictList.Count);
            }
            this.changeDirector.ClearAutoSyncRollback();
            this.PostProcessUpdates(insertedItems, deletedItems);
        }

        private void PostProcessUpdates(List<TrackedObject> insertedItems, List<TrackedObject> deletedItems)
        {
            foreach (TrackedObject to in deletedItems)
            {
                this.services.RemoveCachedObjectLike(to.Type, to.Original);
                this.ClearForeignKeyReferences(to);
            }
            foreach (TrackedObject trackedObject in insertedItems)
            {
                if (this.services.InsertLookupCachedObject(trackedObject.Type, trackedObject.Current) != trackedObject.Current)
                    throw new DuplicateKeyException(trackedObject.Current, Strings.DatabaseGeneratedAlreadyExistingKey);
                trackedObject.InitializeDeferredLoaders();
            }
        }

        private void ClearForeignKeyReferences(TrackedObject to)
        {
            foreach (MetaAssociation metaAssociation in to.Type.Associations)
            {
                if (metaAssociation.IsForeignKey)
                {
                    if (metaAssociation.OtherMember != null && metaAssociation.OtherKeyIsPrimaryKey)
                    {
                        object[] foreignKeyValues = CommonDataServices.GetForeignKeyValues(metaAssociation, to.Current);
                        object instance = this.services.IdentityManager.Find(metaAssociation.OtherType, foreignKeyValues);
                        if (instance != null)
                        {
                            if (metaAssociation.OtherMember.Association.IsMany)
                            {
                                IList list = metaAssociation.OtherMember.MemberAccessor.GetBoxedValue(instance) as IList;
                                if (list != null && !list.IsFixedSize)
                                {
                                    list.Remove(to.Current);
                                    ChangeProcessor.ClearForeignKeysHelper(metaAssociation, to.Current);
                                }
                            }
                            else
                            {
                                metaAssociation.OtherMember.MemberAccessor.SetBoxedValue(ref instance, (object)null);
                                ChangeProcessor.ClearForeignKeysHelper(metaAssociation, to.Current);
                            }
                        }
                    }
                    else
                        ChangeProcessor.ClearForeignKeysHelper(metaAssociation, to.Current);
                }
            }
        }

        private static void ClearForeignKeysHelper(MetaAssociation assoc, object trackedInstance)
        {
            MetaDataMember thisMember = assoc.ThisMember;
            if (thisMember.IsDeferred && !thisMember.StorageAccessor.HasAssignedValue(trackedInstance) && !thisMember.StorageAccessor.HasLoadedValue(trackedInstance))
                thisMember.DeferredSourceAccessor.SetBoxedValue(ref trackedInstance, (object)null);
            thisMember.MemberAccessor.SetBoxedValue(ref trackedInstance, (object)null);
            int index = 0;
            for (int count = assoc.ThisKey.Count; index < count; ++index)
            {
                MetaDataMember metaDataMember = assoc.ThisKey[index];
                if (metaDataMember.CanBeNull)
                    metaDataMember.StorageAccessor.SetBoxedValue(ref trackedInstance, (object)null);
            }
        }

        private static void ValidateAll(IEnumerable<TrackedObject> list)
        {
            foreach (TrackedObject trackedObject in list)
            {
                if (trackedObject.IsNew)
                {
                    trackedObject.SynchDependentData();
                    if (trackedObject.Type.HasAnyValidateMethod)
                        ChangeProcessor.SendOnValidate(trackedObject.Type, trackedObject, ChangeAction.Insert);
                }
                else if (trackedObject.IsDeleted)
                {
                    if (trackedObject.Type.HasAnyValidateMethod)
                        ChangeProcessor.SendOnValidate(trackedObject.Type, trackedObject, ChangeAction.Delete);
                }
                else if (trackedObject.IsPossiblyModified)
                {
                    trackedObject.SynchDependentData();
                    if (trackedObject.IsModified && trackedObject.Type.HasAnyValidateMethod)
                        ChangeProcessor.SendOnValidate(trackedObject.Type, trackedObject, ChangeAction.Update);
                }
            }
        }

        private static void SendOnValidate(MetaType type, TrackedObject item, ChangeAction changeAction)
        {
            if (type == null)
                return;
            ChangeProcessor.SendOnValidate(type.InheritanceBase, item, changeAction);
            if (!(type.OnValidateMethod != (MethodInfo)null))
                return;
            try
            {
                MethodInfo onValidateMethod = type.OnValidateMethod;
                object current = item.Current;
                object[] parameters = new object[1];
                int index = 0;
                // ISSUE: variable of a boxed type
                var local = (Enum)changeAction;
                parameters[index] = (object)local;
                onValidateMethod.Invoke(current, parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        internal string GetChangeText()
        {
            this.ObserveUntrackedObjects();
            this.ApplyInferredDeletions();
            this.BuildEdgeMaps();
            StringBuilder appendTo = new StringBuilder();
            foreach (TrackedObject trackedObject in this.GetOrderedList())
            {
                if (trackedObject.IsNew)
                {
                    trackedObject.SynchDependentData();
                    this.changeDirector.AppendInsertText(trackedObject, appendTo);
                }
                else if (trackedObject.IsDeleted)
                    this.changeDirector.AppendDeleteText(trackedObject, appendTo);
                else if (trackedObject.IsPossiblyModified)
                {
                    trackedObject.SynchDependentData();
                    if (trackedObject.IsModified)
                        this.changeDirector.AppendUpdateText(trackedObject, appendTo);
                }
            }
            return appendTo.ToString();
        }

        internal ChangeSet GetChangeSet()
        {
            List<object> list1 = new List<object>();
            List<object> list2 = new List<object>();
            List<object> list3 = new List<object>();
            this.ObserveUntrackedObjects();
            this.ApplyInferredDeletions();
            foreach (TrackedObject trackedObject in this.tracker.GetInterestingObjects())
            {
                if (trackedObject.IsNew)
                {
                    trackedObject.SynchDependentData();
                    list1.Add(trackedObject.Current);
                }
                else if (trackedObject.IsDeleted)
                    list2.Add(trackedObject.Current);
                else if (trackedObject.IsPossiblyModified)
                {
                    trackedObject.SynchDependentData();
                    if (trackedObject.IsModified)
                        list3.Add(trackedObject.Current);
                }
            }
            return new ChangeSet(list1.AsReadOnly(), list2.AsReadOnly(), list3.AsReadOnly());
        }

        private static void CheckForInvalidChanges(TrackedObject tracked)
        {
            foreach (MetaDataMember mm in tracked.Type.PersistentDataMembers)
            {
                if ((mm.IsPrimaryKey || mm.IsDbGenerated || mm.IsVersion) && tracked.HasChangedValue(mm))
                {
                    if (mm.IsPrimaryKey)
                        throw Error.IdentityChangeNotAllowed((object)mm.Name, (object)tracked.Type.Name);
                    throw Error.DbGeneratedChangeNotAllowed((object)mm.Name, (object)tracked.Type.Name);
                }
            }
        }

        private static ChangeConflictException CreateChangeConflictException(int totalUpdatesAttempted, int failedUpdates)
        {
            string message = Strings.RowNotFoundOrChanged;
            if (totalUpdatesAttempted > 1)
                message = Strings.UpdatesFailedMessage((object)failedUpdates, (object)totalUpdatesAttempted);
            return new ChangeConflictException(message);
        }

        internal void TrackUntrackedObjects()
        {
            Dictionary<object, object> visited = new Dictionary<object, object>();
            foreach (TrackedObject trackedObject in new List<TrackedObject>(this.tracker.GetInterestingObjects()))
                this.TrackUntrackedObjects(trackedObject.Type, trackedObject.Current, visited);
        }

        internal void ApplyInferredDeletions()
        {
            foreach (TrackedObject trackedObject in this.tracker.GetInterestingObjects())
            {
                if (trackedObject.CanInferDelete())
                {
                    if (trackedObject.IsNew)
                        trackedObject.ConvertToRemoved();
                    else if (trackedObject.IsPossiblyModified || trackedObject.IsModified)
                        trackedObject.ConvertToDeleted();
                }
            }
        }

        private void TrackUntrackedObjects(MetaType type, object item, Dictionary<object, object> visited)
        {
            if (visited.ContainsKey(item))
                return;
            Dictionary<object, object> dictionary = visited;
            object key = item;
            dictionary.Add(key, key);
            TrackedObject trackedObject1 = this.tracker.GetTrackedObject(item);
            if (trackedObject1 == null)
            {
                trackedObject1 = this.tracker.Track(item);
                trackedObject1.ConvertToNew();
            }
            else if (trackedObject1.IsDead || trackedObject1.IsRemoved)
                return;
            foreach (RelatedItem relatedItem in this.services.GetParents(type, item))
                this.TrackUntrackedObjects(relatedItem.Type, relatedItem.Item, visited);
            if (trackedObject1.IsNew)
            {
                trackedObject1.InitializeDeferredLoaders();
                TrackedObject trackedObject2 = trackedObject1;
                ReadOnlyCollection<MetaDataMember> identityMembers = trackedObject2.Type.IdentityMembers;
                if (!trackedObject2.IsPendingGeneration((IEnumerable<MetaDataMember>)identityMembers))
                {
                    trackedObject1.SynchDependentData();
                    object obj = this.services.InsertLookupCachedObject(trackedObject1.Type, item);
                    if (obj != item)
                    {
                        TrackedObject trackedObject3 = this.tracker.GetTrackedObject(obj);
                        if (trackedObject3.IsDeleted || trackedObject3.CanInferDelete())
                        {
                            trackedObject1.ConvertToPossiblyModified(trackedObject3.Original);
                            trackedObject3.ConvertToDead();
                            this.services.RemoveCachedObjectLike(trackedObject1.Type, item);
                            this.services.InsertLookupCachedObject(trackedObject1.Type, item);
                        }
                        else if (!trackedObject3.IsDead)
                            throw new DuplicateKeyException(item, Strings.CantAddAlreadyExistingKey);
                    }
                }
                else
                {
                    object cachedObjectLike = this.services.GetCachedObjectLike(trackedObject1.Type, item);
                    if (cachedObjectLike != null)
                    {
                        TrackedObject trackedObject3 = this.tracker.GetTrackedObject(cachedObjectLike);
                        if (trackedObject3.IsDeleted || trackedObject3.CanInferDelete())
                        {
                            trackedObject1.ConvertToPossiblyModified(trackedObject3.Original);
                            trackedObject3.ConvertToDead();
                            this.services.RemoveCachedObjectLike(trackedObject1.Type, item);
                            this.services.InsertLookupCachedObject(trackedObject1.Type, item);
                        }
                    }
                }
            }
            foreach (RelatedItem relatedItem in this.services.GetChildren(type, item))
                this.TrackUntrackedObjects(relatedItem.Type, relatedItem.Item, visited);
        }

        internal void ObserveUntrackedObjects()
        {
            Dictionary<object, object> visited = new Dictionary<object, object>();
            foreach (TrackedObject trackedObject in new List<TrackedObject>(this.tracker.GetInterestingObjects()))
                this.ObserveUntrackedObjects(trackedObject.Type, trackedObject.Current, visited);
        }

        private void ObserveUntrackedObjects(MetaType type, object item, Dictionary<object, object> visited)
        {
            if (visited.ContainsKey(item))
                return;
            Dictionary<object, object> dictionary = visited;
            object key = item;
            dictionary.Add(key, key);
            TrackedObject trackedObject1 = this.tracker.GetTrackedObject(item);
            if (trackedObject1 == null)
            {
                trackedObject1 = this.tracker.Track(item);
                trackedObject1.ConvertToNew();
            }
            else if (trackedObject1.IsDead || trackedObject1.IsRemoved)
                return;
            foreach (RelatedItem relatedItem in this.services.GetParents(type, item))
                this.ObserveUntrackedObjects(relatedItem.Type, relatedItem.Item, visited);
            if (trackedObject1.IsNew)
            {
                TrackedObject trackedObject2 = trackedObject1;
                ReadOnlyCollection<MetaDataMember> identityMembers = trackedObject2.Type.IdentityMembers;
                if (!trackedObject2.IsPendingGeneration((IEnumerable<MetaDataMember>)identityMembers))
                    trackedObject1.SynchDependentData();
            }
            foreach (RelatedItem relatedItem in this.services.GetChildren(type, item))
                this.ObserveUntrackedObjects(relatedItem.Type, relatedItem.Item, visited);
        }

        private TrackedObject GetOtherItem(MetaAssociation assoc, object instance)
        {
            if (instance == null)
                return (TrackedObject)null;
            object obj = (object)null;
            if (assoc.ThisMember.StorageAccessor.HasAssignedValue(instance) || assoc.ThisMember.StorageAccessor.HasLoadedValue(instance))
                obj = assoc.ThisMember.MemberAccessor.GetBoxedValue(instance);
            else if (assoc.OtherKeyIsPrimaryKey)
            {
                object[] foreignKeyValues = CommonDataServices.GetForeignKeyValues(assoc, instance);
                obj = this.services.GetCachedObject(assoc.OtherType, foreignKeyValues);
            }
            if (obj == null)
                return (TrackedObject)null;
            return this.tracker.GetTrackedObject(obj);
        }

        private bool HasAssociationChanged(MetaAssociation assoc, TrackedObject item)
        {
            if (item.Original != null && item.Current != null)
            {
                if (assoc.ThisMember.StorageAccessor.HasAssignedValue(item.Current) || assoc.ThisMember.StorageAccessor.HasLoadedValue(item.Current))
                    return this.GetOtherItem(assoc, item.Current) != this.GetOtherItem(assoc, item.Original);
                object[] foreignKeyValues1 = CommonDataServices.GetForeignKeyValues(assoc, item.Current);
                object[] foreignKeyValues2 = CommonDataServices.GetForeignKeyValues(assoc, item.Original);
                int index = 0;
                for (int length = foreignKeyValues1.Length; index < length; ++index)
                {
                    if (!object.Equals(foreignKeyValues1[index], foreignKeyValues2[index]))
                        return true;
                }
            }
            return false;
        }

        private void BuildEdgeMaps()
        {
            this.currentParentEdges.Clear();
            this.originalChildEdges.Clear();
            this.originalChildReferences.Clear();
            foreach (TrackedObject trackedObject in new List<TrackedObject>(this.tracker.GetInterestingObjects()))
            {
                bool isNew = trackedObject.IsNew;
                foreach (MetaAssociation assoc in trackedObject.Type.Associations)
                {
                    if (assoc.IsForeignKey)
                    {
                        TrackedObject otherItem1 = this.GetOtherItem(assoc, trackedObject.Current);
                        TrackedObject otherItem2 = this.GetOtherItem(assoc, trackedObject.Original);
                        bool flag1 = otherItem1 != null && otherItem1.IsDeleted || otherItem2 != null && otherItem2.IsDeleted;
                        bool flag2 = otherItem1 != null && otherItem1.IsNew;
                        if (isNew | flag1 | flag2 || this.HasAssociationChanged(assoc, trackedObject))
                        {
                            if (otherItem1 != null)
                                this.currentParentEdges.Add(assoc, trackedObject, otherItem1);
                            if (otherItem2 != null)
                            {
                                if (assoc.IsUnique)
                                    this.originalChildEdges.Add(assoc, otherItem2, trackedObject);
                                this.originalChildReferences.Add(otherItem2, trackedObject);
                            }
                        }
                    }
                }
            }
        }

        private List<TrackedObject> GetOrderedList()
        {
            List<TrackedObject> objects = System.Linq.Enumerable.ToList<TrackedObject>(this.tracker.GetInterestingObjects());
            List<int> list1 = System.Linq.Enumerable.ToList<int>(System.Linq.Enumerable.Range(0, objects.Count));
            Comparison<int> comparison = (Comparison<int>)((x, y) => ChangeProcessor.Compare(objects[x], x, objects[y], y));
            list1.Sort(comparison);
            Func<int, TrackedObject> selector = (Func<int, TrackedObject>)(i => objects[i]);
            List<TrackedObject> list2 = System.Linq.Enumerable.ToList<TrackedObject>(System.Linq.Enumerable.Select<int, TrackedObject>((IEnumerable<int>)list1, selector));
            Dictionary<TrackedObject, ChangeProcessor.VisitState> visited = new Dictionary<TrackedObject, ChangeProcessor.VisitState>();
            List<TrackedObject> list3 = new List<TrackedObject>();
            foreach (TrackedObject trackedObject in list2)
                this.BuildDependencyOrderedList(trackedObject, list3, visited);
            return list3;
        }

        private static int Compare(TrackedObject x, int xOrdinal, TrackedObject y, int yOrdinal)
        {
            if (x == y)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            int num1 = x.IsNew ? 0 : (x.IsDeleted ? 2 : 1);
            int num2 = y.IsNew ? 0 : (y.IsDeleted ? 2 : 1);
            if (num1 < num2)
                return -1;
            if (num1 > num2)
                return 1;
            if (x.IsNew)
                return xOrdinal.CompareTo(yOrdinal);
            if (x.Type != y.Type)
                return string.CompareOrdinal(x.Type.Type.FullName, y.Type.Type.FullName);
            foreach (MetaDataMember metaDataMember in x.Type.IdentityMembers)
            {
                object boxedValue1 = metaDataMember.StorageAccessor.GetBoxedValue(x.Current);
                object boxedValue2 = metaDataMember.StorageAccessor.GetBoxedValue(y.Current);
                if (boxedValue1 == null)
                {
                    if (boxedValue2 != null)
                        return -1;
                }
                else
                {
                    IComparable comparable = boxedValue1 as IComparable;
                    if (comparable != null)
                    {
                        int num3 = comparable.CompareTo(boxedValue2);
                        if (num3 != 0)
                            return num3;
                    }
                }
            }
            return xOrdinal.CompareTo(yOrdinal);
        }

        private void BuildDependencyOrderedList(TrackedObject item, List<TrackedObject> list, Dictionary<TrackedObject, ChangeProcessor.VisitState> visited)
        {
            ChangeProcessor.VisitState visitState;
            if (visited.TryGetValue(item, out visitState))
            {
                if (visitState == ChangeProcessor.VisitState.Before)
                    throw Error.CycleDetected();
            }
            else
            {
                visited[item] = ChangeProcessor.VisitState.Before;
                if (item.IsInteresting)
                {
                    if (item.IsDeleted)
                    {
                        foreach (TrackedObject trackedObject in this.originalChildReferences[item])
                        {
                            if (trackedObject != item)
                                this.BuildDependencyOrderedList(trackedObject, list, visited);
                        }
                    }
                    else
                    {
                        foreach (MetaAssociation index1 in item.Type.Associations)
                        {
                            if (index1.IsForeignKey)
                            {
                                TrackedObject index2 = this.currentParentEdges[index1, item];
                                if (index2 != null)
                                {
                                    if (index2.IsNew)
                                    {
                                        if (index2 != item || item.Type.DBGeneratedIdentityMember != null)
                                            this.BuildDependencyOrderedList(index2, list, visited);
                                    }
                                    else if (index1.IsUnique || index1.ThisKeyIsPrimaryKey)
                                    {
                                        TrackedObject trackedObject = this.originalChildEdges[index1, index2];
                                        if (trackedObject != null && index2 != item)
                                            this.BuildDependencyOrderedList(trackedObject, list, visited);
                                    }
                                }
                            }
                        }
                    }
                    list.Add(item);
                }
                visited[item] = ChangeProcessor.VisitState.After;
            }
        }

        private enum VisitState
        {
            Before,
            After,
        }

        private class EdgeMap
        {
            private Dictionary<MetaAssociation, Dictionary<TrackedObject, TrackedObject>> associations;

            internal TrackedObject this[MetaAssociation assoc, TrackedObject from]
            {
                get
                {
                    Dictionary<TrackedObject, TrackedObject> dictionary;
                    TrackedObject trackedObject;
                    if (this.associations.TryGetValue(assoc, out dictionary) && dictionary.TryGetValue(from, out trackedObject))
                        return trackedObject;
                    return (TrackedObject)null;
                }
            }

            internal EdgeMap()
            {
                this.associations = new Dictionary<MetaAssociation, Dictionary<TrackedObject, TrackedObject>>();
            }

            internal void Add(MetaAssociation assoc, TrackedObject from, TrackedObject to)
            {
                Dictionary<TrackedObject, TrackedObject> dictionary;
                if (!this.associations.TryGetValue(assoc, out dictionary))
                {
                    dictionary = new Dictionary<TrackedObject, TrackedObject>();
                    this.associations.Add(assoc, dictionary);
                }
                dictionary.Add(from, to);
            }

            internal void Clear()
            {
                this.associations.Clear();
            }
        }

        private class ReferenceMap
        {
            private static TrackedObject[] Empty = new TrackedObject[0];
            private Dictionary<TrackedObject, List<TrackedObject>> references;

            internal IEnumerable<TrackedObject> this[TrackedObject from]
            {
                get
                {
                    List<TrackedObject> list;
                    if (this.references.TryGetValue(from, out list))
                        return (IEnumerable<TrackedObject>)list;
                    return (IEnumerable<TrackedObject>)ChangeProcessor.ReferenceMap.Empty;
                }
            }

            internal ReferenceMap()
            {
                this.references = new Dictionary<TrackedObject, List<TrackedObject>>();
            }

            internal void Add(TrackedObject from, TrackedObject to)
            {
                List<TrackedObject> list;
                if (!this.references.TryGetValue(from, out list))
                {
                    list = new List<TrackedObject>();
                    this.references.Add(from, list);
                }
                if (list.Contains(to))
                    return;
                list.Add(to);
            }

            internal void Clear()
            {
                this.references.Clear();
            }
        }
    }

    /// <summary>
    /// 描述在将更改提交到数据库时实体将具有的更改的类型。
    /// </summary>
    public enum ChangeAction
    {
        None,
        Delete,
        Insert,
        Update,
    }

    /// <summary>
    /// 当由于客户端上次读取数据库值后这些值已被更新而导致更新失败时引发。
    /// </summary>
    public class ChangeConflictException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.ChangeConflictException"/> 类的新实例。
        /// </summary>
        public ChangeConflictException()
        {
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.ChangeConflictException"/> 类的新实例并指定用于说明异常的消息。
        /// </summary>
        /// <param name="message">当引发异常时要公开的消息。</param>
        public ChangeConflictException(string message)
          : base(message)
        {
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.ChangeConflictException"/> 类的新实例，并指定用于说明异常的消息以及导致此异常的异常。
        /// </summary>
        /// <param name="message">当引发异常时要公开的消息。</param><param name="innerException">指定 <see cref="T:System.Data.Linq.ChangeConflictException"/> 是其结果的异常。</param>
        public ChangeConflictException(string message, Exception innerException)
          : base(message, innerException)
        {
        }
    }
}
