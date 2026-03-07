-- drop table beatles_songs
-- select * from beatles_songs

-- Create the thoughts table
create table beatles_songs (
  id varchar(1024) primary key,
  album varchar(1024) not null,
  title varchar(1024) not null,
  year int not null,
  text text not null,
  embedding vector(1536),
  created_at timestamptz default now(),
  updated_at timestamptz default now()
);

-- Index for fast vector similarity search
create index on beatles_songs using hnsw (embedding vector_cosine_ops);


-- Index for date range queries
create index on beatles_songs (created_at desc);

-- Auto-update the updated_at timestamp
create or replace function update_updated_at()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql;

create trigger beatles_songs_updated_at
  before update on beatles_songs
  for each row
  execute function update_updated_at();

  