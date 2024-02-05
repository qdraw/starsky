dotnet tool install --global protobuf-net.Protogen --version 3.2.12

cat << \EOF >> ~/.zprofile
# Add .NET Core SDK tools
export PATH="$PATH:/Users/dion/.dotnet/tools"
EOF

And run `zsh -l` to make it available for current session.

git clone https://github.com/open-telemetry/opentelemetry-proto.git

cd opentelemetry-proto

protogen opentelemetry/proto/trace/v1/trace.proto --csharp_out=temp
protogen opentelemetry/proto/resource/v1/resource.proto --csharp_out=temp
protogen opentelemetry/proto/common/v1/common.proto --csharp_out=temp