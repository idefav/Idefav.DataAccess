using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示映射信息的源。
    /// </summary>
    public abstract class MappingSource
    {
        private MetaModel primaryModel;
        private ReaderWriterLock rwlock;
        private Dictionary<Type, MetaModel> secondaryModels;

        /// <summary>
        /// 返回映射模型。
        /// </summary>
        /// 
        /// <returns>
        /// 与此映射源关联的映射模型。
        /// </returns>
        /// <param name="dataContextType">要返回的模型的 <see cref="T:System.Data.Linq.DataContext"/> 的类型。</param>
        public MetaModel GetModel(Type dataContextType)
        {
            if (dataContextType == (Type)null)
                throw Error.ArgumentNull("dataContextType");
            MetaModel metaModel1 = (MetaModel)null;
            if (this.primaryModel == null)
            {
                metaModel1 = this.CreateModel(dataContextType);
                Interlocked.CompareExchange<MetaModel>(ref this.primaryModel, metaModel1, (MetaModel)null);
            }
            if (this.primaryModel.ContextType == dataContextType)
                return this.primaryModel;
            if (this.secondaryModels == null)
                Interlocked.CompareExchange<Dictionary<Type, MetaModel>>(ref this.secondaryModels, new Dictionary<Type, MetaModel>(), (Dictionary<Type, MetaModel>)null);
            if (this.rwlock == null)
                Interlocked.CompareExchange<ReaderWriterLock>(ref this.rwlock, new ReaderWriterLock(), (ReaderWriterLock)null);
            this.rwlock.AcquireReaderLock(-1);
            MetaModel metaModel2;
            try
            {
                if (this.secondaryModels.TryGetValue(dataContextType, out metaModel2))
                    return metaModel2;
            }
            finally
            {
                this.rwlock.ReleaseReaderLock();
            }
            this.rwlock.AcquireWriterLock(-1);
            try
            {
                if (this.secondaryModels.TryGetValue(dataContextType, out metaModel2))
                    return metaModel2;
                if (metaModel1 == null)
                    metaModel1 = this.CreateModel(dataContextType);
                this.secondaryModels.Add(dataContextType, metaModel1);
            }
            finally
            {
                this.rwlock.ReleaseWriterLock();
            }
            return metaModel1;
        }

        /// <summary>
        /// 创建新的映射模型。
        /// </summary>
        /// 
        /// <returns>
        /// 为匹配当前的映射架构而创建的元模型。
        /// </returns>
        /// <param name="dataContextType">映射要基于的 <see cref="T:System.Data.Linq.DataContext"/> 的类型。</param>
        protected abstract MetaModel CreateModel(Type dataContextType);
    }
}
