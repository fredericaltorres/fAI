-- Semantic search function
-- drop function match_thoughts(query_embedding, match_threshold, match_count);
-- using c# and supabase and the supabase nuget package
create or replace function search_beatles_songs(
  query_embedding vector(1536),
  match_threshold float default 0.2,
  match_count int default 10
)
returns table (
  id varchar(1024) ,
  text text,
  similarity float,
  created_at timestamptz
)
language plpgsql
as $$
begin
  return query
  select
    t.id,
    t.text,
    1 - (t.embedding <=> query_embedding) as similarity,
    t.created_at
  from beatles_songs t
  where 1 - (t.embedding <=> query_embedding) > match_threshold    
  order by t.embedding <=> query_embedding
  limit match_count;
end;
$$;
