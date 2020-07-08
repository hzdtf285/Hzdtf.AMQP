using Hzdtf.BusinessDemo.Model.Standard;
using Hzdtf.Utility.Standard.Model.Return;
using System;

namespace Hzdtf.BusinessDemo.Contract.Standard
{
    /// <summary>
    /// 人服务
    /// @ 黄振东
    /// </summary>
    public class PersonService : IPersonService
    {
        /// <summary>
        /// 根据ID获取人信息
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>返回信息</returns>
        public ReturnInfo<PersonInfo> Get(int id)
        {
            return new ReturnInfo<PersonInfo>()
            {
                Data = new PersonInfo()
                {
                    Id = id,
                    Name = "张三"
                }
            };
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns>返回信息</returns>
        public ReturnInfo<PersonInfo[]> Query()
        {
            return new ReturnInfo<PersonInfo[]>()
            {
                Data = new PersonInfo[]
                {
                    new PersonInfo()
                    {
                        Id = 1,
                        Name = "张三"
                    },
                     new PersonInfo()
                    {
                        Id = 2,
                        Name = "李四"
                    }
                }
            };
        }
    }
}
