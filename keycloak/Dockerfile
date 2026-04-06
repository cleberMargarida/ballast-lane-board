FROM quay.io/keycloak/keycloak:26.2

COPY ballast-lane-board-realm.json /opt/keycloak/data/import/
COPY themes/ /opt/keycloak/themes/

ENV KC_DB=postgres
ENV KC_CACHE=local

RUN /opt/keycloak/bin/kc.sh build

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--optimized", "--import-realm"]
