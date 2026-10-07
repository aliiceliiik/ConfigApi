namespace ConfigApi.Context.Search;

public static class ProductIndexDefinition
{
    public const string Json = """
    {
      "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "analysis": {
          "filter": {
            "tr_stop": { "type": "stop", "stopwords": "_turkish_" },
            "tr_stemmer": { "type": "stemmer", "language": "turkish" },
            "tr_edge": { "type": "edge_ngram", "min_gram": 2, "max_gram": 20 }
          },
          "analyzer": {
            "tr_analyzer": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": ["lowercase", "tr_stop", "tr_stemmer", "asciifolding"]
            },
            "tr_autocomplete": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": ["lowercase", "asciifolding", "tr_edge"]
            },
            "tr_autocomplete_search": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": ["lowercase", "asciifolding"]
            }
          }
        }
      },
      "mappings": {
        "properties": {
          "id":          { "type": "keyword" },
          "tenantId":    { "type": "keyword" },
          "name": {
            "type": "text",
            "analyzer": "tr_analyzer",
            "fields": {
              "keyword":      { "type": "keyword", "ignore_above": 256 },
              "autocomplete": {
                "type": "text",
                "analyzer": "tr_autocomplete",
                "search_analyzer": "tr_autocomplete_search"
              }
            }
          },
          "description": { "type": "text", "analyzer": "tr_analyzer" },
          "price":       { "type": "double" },
          "stock":       { "type": "integer" },
          "isActive":    { "type": "boolean" },
          "createdAt":   { "type": "date" }
        }
      }
    }
    """;
}
