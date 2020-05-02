using Microsoft.AspNetCore.Mvc;
using System;

namespace AspNetCore.Specification.UI
{
    //[FromQuery]
    public class WebApiParamsDto<T>
    {
        public string Fields { get; set; }

        [ModelBinder(BinderType = typeof(UserFieldsModelBinder), Name = nameof(Fields))]
        public UserFieldsSpecification<T> FieldsSpecification { get; set; }
        //public UserFieldsSpecification<T> FieldsSpecification => new Lazy<UserFieldsSpecification<T>>(() => UserFieldsSpecification.Create<T>(Fields)).Value;

        public string Include { get; set; }

        [ModelBinder(BinderType = typeof(UserIncludeModelBinder), Name = nameof(Include))]
        public IncludeSpecification<T> IncludeSpecification { get; set; }
        //public IncludeSpecification<T> IncludeSpecification => new Lazy<IncludeSpecification<T>>(() => UserIncludeSpecification.Create<T>(Include)).Value;
    }

    public class WebApiParamsDto
    {
        public string Fields { get; set; }

        public UserFieldsSpecification FieldsSpecification(Type type) => UserFieldsSpecification.Create(type, Fields);

        public string Include { get; set; }

        public IncludeSpecification IncludeSpecification(Type type) => UserIncludeSpecification.Create(type, Include);
    }
}
