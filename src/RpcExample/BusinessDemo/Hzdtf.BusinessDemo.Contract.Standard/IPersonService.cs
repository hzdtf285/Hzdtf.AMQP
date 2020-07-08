using Hzdtf.BusinessDemo.Model.Standard;
using Hzdtf.Utility.Standard.Model.Return;
using System;

namespace Hzdtf.BusinessDemo.Contract.Standard
{
    /// <summary>
    /// 人服务接口
    /// @ 黄振东
    /// </summary>
    public interface IPersonService
    {
        /// <summary>
        /// 根据ID获取人信息
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>返回信息</returns>
        ReturnInfo<PersonInfo> Get(int id);

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns>返回信息</returns>
        ReturnInfo<PersonInfo[]> Query();
    }
}
