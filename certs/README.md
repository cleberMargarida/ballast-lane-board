# TLS Certificates

Place `cert.pem` and `key.pem` in this directory before starting the stack.
They are mounted read-only into the Nginx container.

## Option A — Self-signed (local testing)

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout certs/key.pem \
  -out certs/cert.pem \
  -subj "/CN=ballast-lane" \
  -addext "subjectAltName=DNS:ballast-lane,DNS:keycloak.ballast-lane"
```

## Option B — Let's Encrypt (Certbot)

```bash
certbot certonly --standalone -d ballast-lane -d keycloak.ballast-lane
cp /etc/letsencrypt/live/ballast-lane/fullchain.pem certs/cert.pem
cp /etc/letsencrypt/live/ballast-lane/privkey.pem   certs/key.pem
```

Remember to renew before expiry (`certbot renew`) and restart Nginx afterward.

## Security

`certs/` is listed in `.gitignore` — never commit private keys to source control.
