using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class ChangeDirector
    {
        internal abstract int Insert(TrackedObject item);

        internal abstract int DynamicInsert(TrackedObject item);

        internal abstract void AppendInsertText(TrackedObject item, StringBuilder appendTo);

        internal abstract int Update(TrackedObject item);

        internal abstract int DynamicUpdate(TrackedObject item);

        internal abstract void AppendUpdateText(TrackedObject item, StringBuilder appendTo);

        internal abstract int Delete(TrackedObject item);

        internal abstract int DynamicDelete(TrackedObject item);

        internal abstract void AppendDeleteText(TrackedObject item, StringBuilder appendTo);

        internal abstract void RollbackAutoSync();

        internal abstract void ClearAutoSyncRollback();

        internal static ChangeDirector CreateChangeDirector(DataContext context)
        {
            return (ChangeDirector)new ChangeDirector.StandardChangeDirector(context);
        }

        internal class StandardChangeDirector : ChangeDirector
        {
            private DataContext context;
            private List<KeyValuePair<TrackedObject, object[]>> syncRollbackItems;

            private List<KeyValuePair<TrackedObject, object[]>> SyncRollbackItems
            {
                get
                {
                    if (this.syncRollbackItems == null)
                        this.syncRollbackItems = new List<KeyValuePair<TrackedObject, object[]>>();
                    return this.syncRollbackItems;
                }
            }

            internal StandardChangeDirector(DataContext context)
            {
                this.context = context;
            }

            internal override int Insert(TrackedObject item)
            {
                if (!(item.Type.Table.InsertMethod != (MethodInfo)null))
                    return this.DynamicInsert(item);
                try
                {
                    MethodInfo insertMethod = item.Type.Table.InsertMethod;
                    DataContext dataContext = this.context;
                    object[] parameters = new object[1];
                    int index = 0;
                    object current = item.Current;
                    parameters[index] = current;
                    insertMethod.Invoke((object)dataContext, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    throw;
                }
                return 1;
            }

            internal override int DynamicInsert(TrackedObject item)
            {
                Expression insertCommand = this.GetInsertCommand(item);
                if (insertCommand.Type == typeof(int))
                    return (int)this.context.Provider.Execute(insertCommand).ReturnValue;
                object[] syncResults = (object[])System.Linq.Enumerable.FirstOrDefault<object>((IEnumerable<object>)this.context.Provider.Execute(insertCommand).ReturnValue);
                if (syncResults == null)
                    throw Error.InsertAutoSyncFailure();
                this.AutoSyncMembers(syncResults, item, ChangeDirector.StandardChangeDirector.UpdateType.Insert, ChangeDirector.StandardChangeDirector.AutoSyncBehavior.ApplyNewAutoSync);
                return 1;
            }

            internal override void AppendInsertText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.InsertMethod != (MethodInfo)null)
                {
                    appendTo.Append(Strings.InsertCallbackComment);
                }
                else
                {
                    Expression insertCommand = this.GetInsertCommand(item);
                    appendTo.Append(this.context.Provider.GetQueryText(insertCommand));
                    appendTo.AppendLine();
                }
            }

            internal override int Update(TrackedObject item)
            {
                if (!(item.Type.Table.UpdateMethod != (MethodInfo)null))
                    return this.DynamicUpdate(item);
                try
                {
                    MethodInfo updateMethod = item.Type.Table.UpdateMethod;
                    DataContext dataContext = this.context;
                    object[] parameters = new object[1];
                    int index = 0;
                    object current = item.Current;
                    parameters[index] = current;
                    updateMethod.Invoke((object)dataContext, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    throw;
                }
                return 1;
            }

            internal override int DynamicUpdate(TrackedObject item)
            {
                Expression updateCommand = this.GetUpdateCommand(item);
                if (updateCommand.Type == typeof(int))
                    return (int)this.context.Provider.Execute(updateCommand).ReturnValue;
                object[] syncResults = (object[])System.Linq.Enumerable.FirstOrDefault<object>((IEnumerable<object>)this.context.Provider.Execute(updateCommand).ReturnValue);
                if (syncResults == null)
                    return 0;
                this.AutoSyncMembers(syncResults, item, ChangeDirector.StandardChangeDirector.UpdateType.Update, ChangeDirector.StandardChangeDirector.AutoSyncBehavior.ApplyNewAutoSync);
                return 1;
            }

            internal override void AppendUpdateText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.UpdateMethod != (MethodInfo)null)
                {
                    appendTo.Append(Strings.UpdateCallbackComment);
                }
                else
                {
                    Expression updateCommand = this.GetUpdateCommand(item);
                    appendTo.Append(this.context.Provider.GetQueryText(updateCommand));
                    appendTo.AppendLine();
                }
            }

            internal override int Delete(TrackedObject item)
            {
                if (!(item.Type.Table.DeleteMethod != (MethodInfo)null))
                    return this.DynamicDelete(item);
                try
                {
                    MethodInfo deleteMethod = item.Type.Table.DeleteMethod;
                    DataContext dataContext = this.context;
                    object[] parameters = new object[1];
                    int index = 0;
                    object current = item.Current;
                    parameters[index] = current;
                    deleteMethod.Invoke((object)dataContext, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    throw;
                }
                return 1;
            }

            internal override int DynamicDelete(TrackedObject item)
            {
                int num = (int)this.context.Provider.Execute(this.GetDeleteCommand(item)).ReturnValue;
                if (num == 0)
                    num = (int?)this.context.Provider.Execute(this.GetDeleteVerificationCommand(item)).ReturnValue ?? -1;
                return num;
            }

            internal override void AppendDeleteText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.DeleteMethod != (MethodInfo)null)
                {
                    appendTo.Append(Strings.DeleteCallbackComment);
                }
                else
                {
                    Expression deleteCommand = this.GetDeleteCommand(item);
                    appendTo.Append(this.context.Provider.GetQueryText(deleteCommand));
                    appendTo.AppendLine();
                }
            }

            internal override void RollbackAutoSync()
            {
                if (this.syncRollbackItems == null)
                    return;
                foreach (KeyValuePair<TrackedObject, object[]> keyValuePair in this.SyncRollbackItems)
                {
                    TrackedObject key = keyValuePair.Key;
                    object[] syncResults = keyValuePair.Value;
                    TrackedObject trackedObject = key;
                    int num1 = trackedObject.IsNew ? 0 : 1;
                    int num2 = 1;
                    this.AutoSyncMembers(syncResults, trackedObject, (ChangeDirector.StandardChangeDirector.UpdateType)num1, (ChangeDirector.StandardChangeDirector.AutoSyncBehavior)num2);
                }
            }

            internal override void ClearAutoSyncRollback()
            {
                this.syncRollbackItems = (List<KeyValuePair<TrackedObject, object[]>>)null;
            }

            private Expression GetInsertCommand(TrackedObject item)
            {
                List<MetaDataMember> autoSyncMembers = ChangeDirector.StandardChangeDirector.GetAutoSyncMembers(item.Type, ChangeDirector.StandardChangeDirector.UpdateType.Insert);
                ParameterExpression parameterExpression1 = Expression.Parameter(item.Type.Table.RowType.Type, "p");
                if (autoSyncMembers.Count > 0)
                {
                    Expression autoSync = this.CreateAutoSync(autoSyncMembers, (Expression)parameterExpression1);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index1 = 0;
                    ParameterExpression parameterExpression2 = parameterExpression1;
                    parameterExpressionArray[index1] = parameterExpression2;
                    LambdaExpression lambdaExpression1 = Expression.Lambda(autoSync, parameterExpressionArray);
                    Type type1 = typeof(DataManipulation);
                    string methodName = "Insert";
                    Type[] typeArguments = new Type[2];
                    int index2 = 0;
                    Type type2 = item.Type.InheritanceRoot.Type;
                    typeArguments[index2] = type2;
                    int index3 = 1;
                    Type type3 = lambdaExpression1.Body.Type;
                    typeArguments[index3] = type3;
                    Expression[] expressionArray = new Expression[2];
                    int index4 = 0;
                    ConstantExpression constantExpression = Expression.Constant(item.Current);
                    expressionArray[index4] = (Expression)constantExpression;
                    int index5 = 1;
                    LambdaExpression lambdaExpression2 = lambdaExpression1;
                    expressionArray[index5] = (Expression)lambdaExpression2;
                    return (Expression)Expression.Call(type1, methodName, typeArguments, expressionArray);
                }
                Type type4 = typeof(DataManipulation);
                string methodName1 = "Insert";
                Type[] typeArguments1 = new Type[1];
                int index6 = 0;
                Type type5 = item.Type.InheritanceRoot.Type;
                typeArguments1[index6] = type5;
                Expression[] expressionArray1 = new Expression[1];
                int index7 = 0;
                ConstantExpression constantExpression1 = Expression.Constant(item.Current);
                expressionArray1[index7] = (Expression)constantExpression1;
                return (Expression)Expression.Call(type4, methodName1, typeArguments1, expressionArray1);
            }

            private Expression CreateAutoSync(List<MetaDataMember> membersToSync, Expression source)
            {
                int num = 0;
                Expression[] expressionArray = new Expression[membersToSync.Count];
                foreach (MetaDataMember metaDataMember in membersToSync)
                    expressionArray[num++] = (Expression)Expression.Convert(this.GetMemberExpression(source, metaDataMember.Member), typeof(object));
                return (Expression)Expression.NewArrayInit(typeof(object), expressionArray);
            }

            private static List<MetaDataMember> GetAutoSyncMembers(MetaType metaType, ChangeDirector.StandardChangeDirector.UpdateType updateType)
            {
                List<MetaDataMember> list = new List<MetaDataMember>();
                foreach (MetaDataMember metaDataMember in (IEnumerable<MetaDataMember>)System.Linq.Enumerable.OrderBy<MetaDataMember, int>((IEnumerable<MetaDataMember>)metaType.PersistentDataMembers, (Func<MetaDataMember, int>)(m => m.Ordinal)))
                {
                    if (updateType == ChangeDirector.StandardChangeDirector.UpdateType.Insert && metaDataMember.AutoSync == AutoSync.OnInsert || updateType == ChangeDirector.StandardChangeDirector.UpdateType.Update && metaDataMember.AutoSync == AutoSync.OnUpdate || metaDataMember.AutoSync == AutoSync.Always)
                        list.Add(metaDataMember);
                }
                return list;
            }

            private void AutoSyncMembers(object[] syncResults, TrackedObject item, ChangeDirector.StandardChangeDirector.UpdateType updateType, ChangeDirector.StandardChangeDirector.AutoSyncBehavior autoSyncBehavior)
            {
                object[] objArray = (object[])null;
                if (syncResults != null)
                {
                    int index = 0;
                    List<MetaDataMember> autoSyncMembers = ChangeDirector.StandardChangeDirector.GetAutoSyncMembers(item.Type, updateType);
                    if (autoSyncBehavior == ChangeDirector.StandardChangeDirector.AutoSyncBehavior.ApplyNewAutoSync)
                        objArray = new object[syncResults.Length];
                    foreach (MetaDataMember metaDataMember in autoSyncMembers)
                    {
                        object obj = syncResults[index];
                        object current = item.Current;
                        MetaAccessor metaAccessor = !(metaDataMember.Member is PropertyInfo) || !((PropertyInfo)metaDataMember.Member).CanWrite ? metaDataMember.StorageAccessor : metaDataMember.MemberAccessor;
                        if (objArray != null)
                            objArray[index] = metaAccessor.GetBoxedValue(current);
                        metaAccessor.SetBoxedValue(ref current, DBConvert.ChangeType(obj, metaDataMember.Type));
                        ++index;
                    }
                }
                if (objArray == null)
                    return;
                this.SyncRollbackItems.Add(new KeyValuePair<TrackedObject, object[]>(item, objArray));
            }

            private Expression GetUpdateCommand(TrackedObject tracked)
            {
                object original = tracked.Original;
                MetaType inheritanceType = tracked.Type.GetInheritanceType(original.GetType());
                MetaType inheritanceRoot = inheritanceType.InheritanceRoot;
                ParameterExpression parameterExpression1 = Expression.Parameter(inheritanceRoot.Type, "p");
                Expression expression1 = (Expression)parameterExpression1;
                if (inheritanceType != inheritanceRoot)
                    expression1 = (Expression)Expression.Convert((Expression)parameterExpression1, inheritanceType.Type);
                Expression expression2 = this.GetUpdateCheck(expression1, tracked);
                if (expression2 != null)
                {
                    Expression body = expression2;
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index = 0;
                    ParameterExpression parameterExpression2 = parameterExpression1;
                    parameterExpressionArray[index] = parameterExpression2;
                    expression2 = (Expression)Expression.Lambda(body, parameterExpressionArray);
                }
                List<MetaDataMember> autoSyncMembers = ChangeDirector.StandardChangeDirector.GetAutoSyncMembers(inheritanceType, ChangeDirector.StandardChangeDirector.UpdateType.Update);
                if (autoSyncMembers.Count > 0)
                {
                    Expression autoSync = this.CreateAutoSync(autoSyncMembers, expression1);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index1 = 0;
                    ParameterExpression parameterExpression2 = parameterExpression1;
                    parameterExpressionArray[index1] = parameterExpression2;
                    LambdaExpression lambdaExpression1 = Expression.Lambda(autoSync, parameterExpressionArray);
                    if (expression2 != null)
                    {
                        Type type1 = typeof(DataManipulation);
                        string methodName = "Update";
                        Type[] typeArguments = new Type[2];
                        int index2 = 0;
                        Type type2 = inheritanceRoot.Type;
                        typeArguments[index2] = type2;
                        int index3 = 1;
                        Type type3 = lambdaExpression1.Body.Type;
                        typeArguments[index3] = type3;
                        Expression[] expressionArray = new Expression[3];
                        int index4 = 0;
                        ConstantExpression constantExpression = Expression.Constant(tracked.Current);
                        expressionArray[index4] = (Expression)constantExpression;
                        int index5 = 1;
                        Expression expression3 = expression2;
                        expressionArray[index5] = expression3;
                        int index6 = 2;
                        LambdaExpression lambdaExpression2 = lambdaExpression1;
                        expressionArray[index6] = (Expression)lambdaExpression2;
                        return (Expression)Expression.Call(type1, methodName, typeArguments, expressionArray);
                    }
                    Type type4 = typeof(DataManipulation);
                    string methodName1 = "Update";
                    Type[] typeArguments1 = new Type[2];
                    int index7 = 0;
                    Type type5 = inheritanceRoot.Type;
                    typeArguments1[index7] = type5;
                    int index8 = 1;
                    Type type6 = lambdaExpression1.Body.Type;
                    typeArguments1[index8] = type6;
                    Expression[] expressionArray1 = new Expression[2];
                    int index9 = 0;
                    ConstantExpression constantExpression1 = Expression.Constant(tracked.Current);
                    expressionArray1[index9] = (Expression)constantExpression1;
                    int index10 = 1;
                    LambdaExpression lambdaExpression3 = lambdaExpression1;
                    expressionArray1[index10] = (Expression)lambdaExpression3;
                    return (Expression)Expression.Call(type4, methodName1, typeArguments1, expressionArray1);
                }
                if (expression2 != null)
                {
                    Type type1 = typeof(DataManipulation);
                    string methodName = "Update";
                    Type[] typeArguments = new Type[1];
                    int index1 = 0;
                    Type type2 = inheritanceRoot.Type;
                    typeArguments[index1] = type2;
                    Expression[] expressionArray = new Expression[2];
                    int index2 = 0;
                    ConstantExpression constantExpression = Expression.Constant(tracked.Current);
                    expressionArray[index2] = (Expression)constantExpression;
                    int index3 = 1;
                    Expression expression3 = expression2;
                    expressionArray[index3] = expression3;
                    return (Expression)Expression.Call(type1, methodName, typeArguments, expressionArray);
                }
                Type type7 = typeof(DataManipulation);
                string methodName2 = "Update";
                Type[] typeArguments2 = new Type[1];
                int index11 = 0;
                Type type8 = inheritanceRoot.Type;
                typeArguments2[index11] = type8;
                Expression[] expressionArray2 = new Expression[1];
                int index12 = 0;
                ConstantExpression constantExpression2 = Expression.Constant(tracked.Current);
                expressionArray2[index12] = (Expression)constantExpression2;
                return (Expression)Expression.Call(type7, methodName2, typeArguments2, expressionArray2);
            }

            private Expression GetUpdateCheck(Expression serverItem, TrackedObject tracked)
            {
                MetaType type = tracked.Type;
                if (type.VersionMember != null)
                    return (Expression)Expression.Equal(this.GetMemberExpression(serverItem, type.VersionMember.Member), this.GetMemberExpression((Expression)Expression.Constant(tracked.Current), type.VersionMember.Member));
                Expression left = (Expression)null;
                foreach (MetaDataMember mm in type.PersistentDataMembers)
                {
                    if (!mm.IsPrimaryKey)
                    {
                        switch (mm.UpdateCheck)
                        {
                            case UpdateCheck.Always:
                                object boxedValue = mm.MemberAccessor.GetBoxedValue(tracked.Original);
                                Expression right = (Expression)Expression.Equal(this.GetMemberExpression(serverItem, mm.Member), (Expression)Expression.Constant(boxedValue, mm.Type));
                                left = left != null ? (Expression)Expression.And(left, right) : right;
                                continue;
                            case UpdateCheck.WhenChanged:
                                if (!tracked.HasChangedValue(mm))
                                    continue;
                                goto case UpdateCheck.Always;
                            default:
                                continue;
                        }
                    }
                }
                return left;
            }

            private Expression GetDeleteCommand(TrackedObject tracked)
            {
                MetaType type1 = tracked.Type;
                MetaType inheritanceRoot = type1.InheritanceRoot;
                ParameterExpression parameterExpression1 = Expression.Parameter(inheritanceRoot.Type, "p");
                Expression serverItem = (Expression)parameterExpression1;
                if (type1 != inheritanceRoot)
                    serverItem = (Expression)Expression.Convert((Expression)parameterExpression1, type1.Type);
                TrackedObject trackedObject = tracked;
                object original = trackedObject.Original;
                object dataCopy = trackedObject.CreateDataCopy(original);
                Expression updateCheck = this.GetUpdateCheck(serverItem, tracked);
                if (updateCheck != null)
                {
                    Expression body = updateCheck;
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index1 = 0;
                    ParameterExpression parameterExpression2 = parameterExpression1;
                    parameterExpressionArray[index1] = parameterExpression2;
                    Expression expression1 = (Expression)Expression.Lambda(body, parameterExpressionArray);
                    Type type2 = typeof(DataManipulation);
                    string methodName = "Delete";
                    Type[] typeArguments = new Type[1];
                    int index2 = 0;
                    Type type3 = inheritanceRoot.Type;
                    typeArguments[index2] = type3;
                    Expression[] expressionArray = new Expression[2];
                    int index3 = 0;
                    ConstantExpression constantExpression = Expression.Constant(dataCopy);
                    expressionArray[index3] = (Expression)constantExpression;
                    int index4 = 1;
                    Expression expression2 = expression1;
                    expressionArray[index4] = expression2;
                    return (Expression)Expression.Call(type2, methodName, typeArguments, expressionArray);
                }
                Type type4 = typeof(DataManipulation);
                string methodName1 = "Delete";
                Type[] typeArguments1 = new Type[1];
                int index5 = 0;
                Type type5 = inheritanceRoot.Type;
                typeArguments1[index5] = type5;
                Expression[] expressionArray1 = new Expression[1];
                int index6 = 0;
                ConstantExpression constantExpression1 = Expression.Constant(dataCopy);
                expressionArray1[index6] = (Expression)constantExpression1;
                return (Expression)Expression.Call(type4, methodName1, typeArguments1, expressionArray1);
            }

            private Expression GetDeleteVerificationCommand(TrackedObject tracked)
            {
                ITable table = this.context.GetTable(tracked.Type.InheritanceRoot.Type);
                ParameterExpression parameterExpression1 = Expression.Parameter(table.ElementType, "p");
                BinaryExpression binaryExpression = Expression.Equal((Expression)parameterExpression1, (Expression)Expression.Constant(tracked.Current));
                ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
                int index1 = 0;
                ParameterExpression parameterExpression2 = parameterExpression1;
                parameterExpressionArray1[index1] = parameterExpression2;
                Expression expression1 = (Expression)Expression.Lambda((Expression)binaryExpression, parameterExpressionArray1);
                Type type1 = typeof(Queryable);
                string methodName1 = "Where";
                Type[] typeArguments1 = new Type[1];
                int index2 = 0;
                Type elementType1 = table.ElementType;
                typeArguments1[index2] = elementType1;
                Expression[] expressionArray1 = new Expression[2];
                int index3 = 0;
                Expression expression2 = table.Expression;
                expressionArray1[index3] = expression2;
                int index4 = 1;
                Expression expression3 = expression1;
                expressionArray1[index4] = expression3;
                Expression expression4 = (Expression)Expression.Call(type1, methodName1, typeArguments1, expressionArray1);
                ConstantExpression constantExpression = Expression.Constant((object)0, typeof(int?));
                ParameterExpression[] parameterExpressionArray2 = new ParameterExpression[1];
                int index5 = 0;
                ParameterExpression parameterExpression3 = parameterExpression1;
                parameterExpressionArray2[index5] = parameterExpression3;
                Expression expression5 = (Expression)Expression.Lambda((Expression)constantExpression, parameterExpressionArray2);
                Type type2 = typeof(Queryable);
                string methodName2 = "Select";
                Type[] typeArguments2 = new Type[2];
                int index6 = 0;
                Type elementType2 = table.ElementType;
                typeArguments2[index6] = elementType2;
                int index7 = 1;
                Type type3 = typeof(int?);
                typeArguments2[index7] = type3;
                Expression[] expressionArray2 = new Expression[2];
                int index8 = 0;
                Expression expression6 = expression4;
                expressionArray2[index8] = expression6;
                int index9 = 1;
                Expression expression7 = expression5;
                expressionArray2[index9] = expression7;
                Expression expression8 = (Expression)Expression.Call(type2, methodName2, typeArguments2, expressionArray2);
                Type type4 = typeof(Queryable);
                string methodName3 = "SingleOrDefault";
                Type[] typeArguments3 = new Type[1];
                int index10 = 0;
                Type type5 = typeof(int?);
                typeArguments3[index10] = type5;
                Expression[] expressionArray3 = new Expression[1];
                int index11 = 0;
                Expression expression9 = expression8;
                expressionArray3[index11] = expression9;
                return (Expression)Expression.Call(type4, methodName3, typeArguments3, expressionArray3);
            }

            private Expression GetMemberExpression(Expression exp, MemberInfo mi)
            {
                FieldInfo field = mi as FieldInfo;
                if (field != (FieldInfo)null)
                    return (Expression)Expression.Field(exp, field);
                PropertyInfo property = (PropertyInfo)mi;
                return (Expression)Expression.Property(exp, property);
            }

            private enum UpdateType
            {
                Insert,
                Update,
                Delete,
            }

            private enum AutoSyncBehavior
            {
                ApplyNewAutoSync,
                RollbackSavedValues,
            }
        }
    }
}
