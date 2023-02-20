# YAML files

[YAML](https://en.wikipedia.org/wiki/YAML) is a human-friendly format that we use for metadata exports and configuration
files because of its simplicity and widespread support. The name originally meant *Yet Another Markup Language*.
Common file extensions are `.yml`and `.yaml`.

Values are represented in the form `key: value` with one entry per line:

```yaml
Type: image
Title: "La Tour Eiffel 🌈"
Year: 2014
# Key-Value Collection Example:
Details:
  Notes: "Bonjour\nla France!" 
  Keywords: 'paris, france' # Comment
```

## Basic Rules ##

- Keys are case-sensitive
- Related values start at the same indentation level
    - Tabs are not allowed for indentation
    - We recommend using 2 spaces, but any number will work as long as it is consistent
- You can generally use all [Unicode](https://home.unicode.org/basic-info/faq/) characters and [Emojis](https://home.unicode.org/emoji/about-emoji/)
    - To avoid ambiguity, it is recommended to enclose text strings in single `'` or double quotes `"`, especially if they contain a colon `:`
    - The difference between single and double quotes is that double quotes support [escape sequences](https://symfony.com/doc/current/components/yaml/yaml_format.html#strings) like `\t` for a tab or `\n` for a new line
- Comments begin with the `#` sign, can start anywhere on a line, and continue until the end of the line

## Multiple Values ##

List are lines that start at the same indentation level and begin with a dash and a space as shown in the example below.
They are commonly used to define service dependencies, exposed network ports, or folders shared between host and
container in `docker-compose.yml` files:

```yaml
services:
  starsky:
    depends_on:
      - mariadb
    ports:
      # "host:container"
      - "2342:2342"
    volumes:
      # "/host/folder:/container/folder"
      - "/photos:/app/storageFolder"
```

## Key-Value Collections ##

Collections of key-value pairs are commonly used for photo metadata details as shown in the first example
and to define environment variables in `docker-compose.yml` files:

```yaml
services:
  mariadb:
    environment:
      MARIADB_AUTO_UPGRADE: "1"
      MARIADB_INITDB_SKIP_TZINFO: "1"
      MARIADB_ROOT_PASSWORD: "Y(&^UIk34"
      MARIADB_DATABASE: starsky
      MARIADB_USER: starsky
      MARIADB_PASSWORD: insecure
```

*[Keys]: the names of values
*[tab]: a tab advances the cursor to the next tab stop
*[key-value]: a key-value pair consists of two related data elements
