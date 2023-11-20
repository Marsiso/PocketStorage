<h1>Pocket Storage</h1>

<h2>Database</h2>
<p>Docker</p>
<code>docker run --name pocket_storage_database -e POSTGRES_DB=pocket_storage -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5433:5432 -d postgres:16</code>
