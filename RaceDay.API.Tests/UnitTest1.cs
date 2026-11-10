using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{ 
    public class UnitTest1
    {
        // USER TESTS

        [Fact]
        public void User_WithValidData_ShouldPassValidation()
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                Password = "TestPassword123!",
                PhoneNumber = "0712345678",
                RoleID = 2
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(user);

            bool isValid = Validator.TryValidateObject(
                user, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void User_WithFirstNameTooLong_ShouldFailValidation()
        {
            var user = new User
            {
                FirstName = new string('A', 51),
                LastName = "Smith",
                Email = "john@example.com",
                Password = "TestPassword123!",
                PhoneNumber = "0712345678",
                RoleID = 2
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(user);

            bool isValid = Validator.TryValidateObject(
                user, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void User_WithEmptyEmail_ShouldFailValidation()
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "",
                Password = "TestPassword123!",
                PhoneNumber = "0712345678",
                RoleID = 2
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(user);

            bool isValid = Validator.TryValidateObject(
                user, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void User_WithPhoneNumberTooLong_ShouldFailValidation()
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                Password = "TestPassword123!",
                PhoneNumber = "0712345678901234",
                RoleID = 2
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(user);

            bool isValid = Validator.TryValidateObject(
                user, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        // ROLE TESTS

        [Fact]
        public void Role_WithValidData_ShouldPassValidation()
        {
            var role = new Role
            {
                RoleID = 1,
                RoleName = "Organiser"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(role);

            bool isValid = Validator.TryValidateObject(
                role, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Role_WithNameTooLong_ShouldFailValidation()
        {
            var role = new Role
            {
                RoleName = new string('A', 51)
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(role);

            bool isValid = Validator.TryValidateObject(
                role, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        // EVENT TESTS

        [Fact]
        public void Event_WithValidData_ShouldPassValidation()
        {
            var eventItem = new Event
            {
                EventName = "Pathfinder Camp",
                Description = "Annual Pathfinder camping event",
                EventDate = new DateTime(2027, 6, 15),
                Location = "Gqeberha",
                EventType = "Camping",
                OrganiserID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(eventItem);

            bool isValid = Validator.TryValidateObject(
                eventItem, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Event_WithNameTooLong_ShouldFailValidation()
        {
            var eventItem = new Event
            {
                EventName = new string('A', 101),
                Description = "Test event",
                EventDate = new DateTime(2027, 6, 15),
                Location = "Gqeberha",
                EventType = "Camping",
                OrganiserID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(eventItem);

            bool isValid = Validator.TryValidateObject(
                eventItem, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void Event_WithDescriptionTooLong_ShouldFailValidation()
        {
            var eventItem = new Event
            {
                EventName = "Test Event",
                Description = new string('A', 256),
                EventDate = new DateTime(2027, 6, 15),
                Location = "Gqeberha",
                EventType = "Camping",
                OrganiserID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(eventItem);

            bool isValid = Validator.TryValidateObject(
                eventItem, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        // CATEGORY TESTS

        [Fact]
        public void Category_WithValidData_ShouldPassValidation()
        {
            var category = new Category
            {
                CategoryName = "Sports",
                EventID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(category);

            bool isValid = Validator.TryValidateObject(
                category, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Category_WithNameTooLong_ShouldFailValidation()
        {
            var category = new Category
            {
                CategoryName = new string('A', 51),
                EventID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(category);

            bool isValid = Validator.TryValidateObject(
                category, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void Category_WithEmptyName_ShouldFailValidation()
        {
            var category = new Category
            {
                CategoryName = "",
                EventID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(category);

            bool isValid = Validator.TryValidateObject(
                category, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        // REGISTRATION TESTS

        [Fact]
        public void Registration_WithValidData_ShouldPassValidation()
        {
            var registration = new Registration
            {
                RegistrationDate = new DateTime(2026, 10, 15),
                UserID = 1,
                EventID = 1,
                CategoryID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(registration);

            bool isValid = Validator.TryValidateObject(
                registration, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Registration_WithDefaultDate_ShouldPassRequiredValidation()
        {
            var registration = new Registration
            {
                RegistrationDate = default,
                UserID = 1,
                EventID = 1,
                CategoryID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(registration);

            bool isValid = Validator.TryValidateObject(
                registration, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Registration_WithDifferentForeignKeyValues_ShouldPassValidation()
        {
            var registration = new Registration
            {
                RegistrationDate = new DateTime(2026, 10, 15),
                UserID = 2,
                EventID = 3,
                CategoryID = 4
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(registration);

            bool isValid = Validator.TryValidateObject(
                registration, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        // RESULT TESTS

        [Fact]
        public void Result_WithValidData_ShouldPassValidation()
        {
            var result = new Result
            {
                FinishTime = new TimeSpan(0, 45, 30),
                Position = 1,
                RegistrationID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(result);

            bool isValid = Validator.TryValidateObject(
                result, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Result_WithZeroPosition_ShouldPassRequiredValidation()
        {
            var result = new Result
            {
                FinishTime = new TimeSpan(0, 45, 30),
                Position = 0,
                RegistrationID = 1
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(result);

            bool isValid = Validator.TryValidateObject(
                result, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Result_WithDifferentRegistrationID_ShouldPassValidation()
        {
            var result = new Result
            {
                FinishTime = new TimeSpan(1, 10, 0),
                Position = 2,
                RegistrationID = 5
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(result);

            bool isValid = Validator.TryValidateObject(
                result, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }
    }
}
