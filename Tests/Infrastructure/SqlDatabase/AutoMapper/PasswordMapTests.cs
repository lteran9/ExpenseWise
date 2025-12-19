using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
{
    public class PasswordMapTests
    {
        [Fact]
        public void EquivalenceMap()
        {
            var dbPassword1 = new PasswordEntity();
            var dbPassword2 = new PasswordEntity();

            Assert.Equivalent(dbPassword1, dbPassword2);

            var password1 = new Password();
            var password2 = new Password();

            Assert.Equivalent(password1, password2);
        }

        [Fact]
        public void AutoMapper_DatabaseToEntity()
        {
            var password =
                new Password()
                {
                    UserId = 1000,
                    Cipher = "The quick brown fox jumped over the lazy dog.",
                    Encrypted = "XYYKJ7tcrU51Xy2ksZw6mp7PgiXvCLonJ0s2CwjLunG1z6j9CGFKaFyUznEbA594"
                };

            var dbPassword =
                new PasswordEntity()
                {
                    UserId = 1000,
                    Cipher = "The quick brown fox jumped over the lazy dog.",
                    Encrypted = "XYYKJ7tcrU51Xy2ksZw6mp7PgiXvCLonJ0s2CwjLunG1z6j9CGFKaFyUznEbA594"
                };

            Assert.Equivalent(password, DatabaseMapper.PasswordMapper.Map<Password>(dbPassword));
        }

        [Fact]
        public void AutoMapper_EntityToDatabase()
        {
            var password =
                new Password()
                {
                    UserId = 1000,
                    Cipher = "The quick brown fox jumped over the lazy dog.",
                    Encrypted = "XYYKJ7tcrU51Xy2ksZw6mp7PgiXvCLonJ0s2CwjLunG1z6j9CGFKaFyUznEbA594"
                };

            var dbPassword =
                new PasswordEntity()
                {
                    UserId = 1000,
                    Cipher = "The quick brown fox jumped over the lazy dog.",
                    Encrypted = "XYYKJ7tcrU51Xy2ksZw6mp7PgiXvCLonJ0s2CwjLunG1z6j9CGFKaFyUznEbA594"
                };

            Assert.Equivalent(dbPassword, DatabaseMapper.PasswordMapper.Map<PasswordEntity>(password));
        }
    }
}
