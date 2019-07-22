using Raven.Client.Documents;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.RavenDB.Analyzers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public class TranslationCacheRepository : ITranslationCacheRepository
    {
        private readonly IDocumentRepository _documentService;

        public TranslationCacheRepository(IDocumentRepository documentService)
        {
            _documentService = documentService;
        }

        public TranslationCache GetTranslationCache(string id)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                return session.Load<TranslationCache>(id);
            }
        }

        public void SaveTranslationCache(TranslationCache translationCache)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                session.Store(translationCache);
                session.SaveChanges();
            }
        }
    }
}
