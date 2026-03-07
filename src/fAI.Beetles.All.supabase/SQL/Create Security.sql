-- Enable Row Level Security
-- alter table BeatlesSongs enable row level security;

-- Service role full access only
create policy "Service role full access"
  on BeatlesSongs
  for all
  using (auth.role() = 'service_role');