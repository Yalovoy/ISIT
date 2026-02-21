using System;
using System.Collections.Generic;

namespace FamilyTreeApp.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Description
        {
            get
            {
                return $"{FullName}\n{BirthDate:dd.MM.yyyy}";
            }
        }

        public int X { get; set; }
        public int Y { get; set; }

        public Person(int id, string fullName, DateTime birthDate)
        {
            Id = id;
            FullName = fullName;
            BirthDate = birthDate;
        }
    }

    public class Relationship
    {
        public int Id { get; set; }
        public int FromPersonId { get; set; }
        public int ToPersonId { get; set; }
        public string RelationType { get; set; } 

        public Relationship(int id, int fromId, int toId, string type)
        {
            Id = id;
            FromPersonId = fromId;
            ToPersonId = toId;
            RelationType = type;
        }
    }

    public class DisplayRelationship
    {
        public Person From { get; set; }
        public Person To { get; set; }
        public string RelationType { get; set; }
    }
}