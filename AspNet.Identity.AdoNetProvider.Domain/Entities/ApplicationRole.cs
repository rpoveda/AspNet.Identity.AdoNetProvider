using System;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.AdoNetProvider.Domain.Entities
{
    public class ApplicationRole : IRole
    {
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ApplicationRole(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public ApplicationRole(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}