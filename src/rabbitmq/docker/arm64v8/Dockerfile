FROM arm64v8/rabbitmq:3.8

COPY src/rabbitmq/init.sh /init.sh
RUN ["chmod", "+x", "/init.sh"]

CMD ["/init.sh"]