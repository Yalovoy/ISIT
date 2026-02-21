using System;
using System.Collections.Generic;
using System.Linq;
using FamilyTreeApp.Models;

namespace FamilyTreeApp.Services
{
    public class FamilyTreeService
    {
        private List<Person> _people;
        private List<Relationship> _relationships;
        private int _nextPersonId;
        private int _nextRelationshipId;

        public event EventHandler DataChanged;

        public FamilyTreeService()
        {
            _people = new List<Person>();
            _relationships = new List<Relationship>();
            _nextPersonId = 1;
            _nextRelationshipId = 1;
        }

        public List<Person> GetAllPeople() => _people;
        public List<Relationship> GetAllRelationships() => _relationships;

        public List<DisplayRelationship> GetDisplayRelationships()
        {
            var result = new List<DisplayRelationship>();

            foreach (var rel in _relationships)
            {
                var from = _people.FirstOrDefault(p => p.Id == rel.FromPersonId);
                var to = _people.FirstOrDefault(p => p.Id == rel.ToPersonId);

                if (from != null && to != null)
                {
                    result.Add(new DisplayRelationship
                    {
                        From = from,
                        To = to,
                        RelationType = rel.RelationType
                    });
                }
            }

            return result;
        }

        public Person AddPerson(string fullName, DateTime birthDate)
        {
            var person = new Person(_nextPersonId++, fullName, birthDate);

            person.X = 400;
            person.Y = 200 + (_people.Count * 60);

            _people.Add(person);

            OnDataChanged();
            return person;
        }

        public void AddRelationship(int fromId, int toId, string type)
        {
            var from = _people.FirstOrDefault(p => p.Id == fromId);
            var to = _people.FirstOrDefault(p => p.Id == toId);

            if (from != null && to != null)
            {
                if (RelationshipExists(fromId, toId, type))
                {
                    return;
                }

                if (type == "супруг")
                {
                    _relationships.Add(new Relationship(_nextRelationshipId++, fromId, toId, "супруг"));
                    _relationships.Add(new Relationship(_nextRelationshipId++, toId, fromId, "супруг"));
                }
                else
                {
                    _relationships.Add(new Relationship(_nextRelationshipId++, fromId, toId, type));
                }

                OnDataChanged();
            }
        }

        private bool RelationshipExists(int fromId, int toId, string type)
        {
            if (type == "супруг")
            {
                return _relationships.Any(r =>
                    (r.FromPersonId == fromId && r.ToPersonId == toId && r.RelationType == "супруг") ||
                    (r.FromPersonId == toId && r.ToPersonId == fromId && r.RelationType == "супруг"));
            }
            else
            {
                return _relationships.Any(r =>
                    r.FromPersonId == fromId && r.ToPersonId == toId && r.RelationType == type);
            }
        }

        public void DeletePerson(int personId)
        {
            var person = _people.FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                _relationships.RemoveAll(r => r.FromPersonId == personId || r.ToPersonId == personId);

                _people.Remove(person);

                OnDataChanged();
            }
        }

        public void ClearAll()
        {
            _people.Clear();
            _relationships.Clear();
            _nextPersonId = 1;
            _nextRelationshipId = 1;
            OnDataChanged();
        }

        public void NotifyDataChanged()
        {
            OnDataChanged();
        }

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}