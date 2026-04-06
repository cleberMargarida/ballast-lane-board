FROM quay.io/keycloak/keycloak:26.2

COPY ballast-lane-board-realm.json /opt/keycloak/data/import/
COPY themes/ /opt/keycloak/themes/

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--import-realm"]
