import React, { Component } from 'react';
import {
  Platform,
  StyleSheet,
  Text,
  View,
  SectionList,
  Image,
  FlatList,
  ActivityIndicator,
  ScrollView,
  Button
} from 'react-native';

// import folderfetch from './folderfetch';


export default class HomeScreen extends React.Component {
  constructor(props){
    super(props);
    const { params } = this.props.navigation.state;
    this.state = { 
       isLoading: true,
       filePath: params ? params.filePath : "/"
    }
  }
  
  // componentWillReceiveProps() {
  //   // if (this.props.viewer && !this.props.navigation.state.params) {
  //   //   this.props.navigation.setParams({imageUrl: this.props.viewer.imageUrl});
  //   // }

  //   // const { filePath } = this.props.navigation.state.params;
  //   // const filePath = this.props.navigation.getParam('filePath', '/2018');

  //   // const { params } = this.props.navigation.state;
  //   // const filePath = params ? params.filePath : "/";

  //   // console.log(filePath);

  //   // console.log("this.props.navigation");

  //   // console.log(this.props.navigation);
  //   // console.log(this.props.navigation);

  //   // const { params } = this.props.navigation.state;
  //   // const filePath = params ? params.filePath : null;
  //   // // const otherParam = params ? params.otherParam : null;


  //   // const { navigate } = this.props.navigation;
    
  //   // console.log("sdfsdfdsfsdf1");

  //   // console.log(filePath);

  //   // console.log(filePath);

  //   console.log("dfsdfnlsdfnlsdkfnlkdsf")
  //   console.log(navigation.state)

  //   // console.log("NAV11: ", this.props.navigation.filePath);
  //   // console.log(this.props.navigation.getParam());

  //   return fetch('http://localhost:5000/api/folder?f=')
  //   .then((response) => response.json())
  //   .then((responseJson) => {

  //     this.setState({
  //       isLoading: false,
  //       dataSource: responseJson
  //     }, function(){
  //     });

  //   })
  //   .catch((error) =>{
  //     console.error(error);
  //   });

  // }


  static navigationOptions = ({ navigation, navigationOptions }) => {
    const { params } = navigation.state;
    return {
      title: params ? params.title : 'Home',
      filePath : params ? params.filePath : '/',
    };
  };
  
  componentDidMount(){

    return fetch('http://localhost:5000/?json=true&f=' + this.state.filePath)
      .then((response) => response.json())
      .then((responseJson) => {

        console.log(responseJson.pageType)

        if(responseJson.pageType === "Archive") {
          this.setState({
            isLoading: false,
            dataSource: responseJson.fileIndexItems
          }, function(){
          });
        }
        if(responseJson.pageType === "DetailView") {
          this.props.navigation.navigate('Details',{
            filePath: this.state.filePath
          })
        }

      })
      .catch((error) =>{
        console.error(error);
      });
  }

  render(){


    if(this.state.isLoading){
      return(
        <View style={{flex: 1, padding: 20}}>
          <ActivityIndicator/>
        </View>
      )
    }



    return(
      <View style={{flex: 1, paddingTop:20}}>

        <FlatList
          data={this.state.dataSource}
          renderItem={({item}) => 
          <Text>
            <Button
              title={"Go to " + item.fileName}
              onPress={() => {
                /* 1. Navigate to the Details route with params */
                this.props.navigation.navigate('Home', {
                  title: item.fileName,
                  filePath: item.filePath,
                });
              }}
            />
          {item.fileName}, {item.filePath}</Text>}
            keyExtractor={(item, index) => index}
        />

      </View>
    );
  }
}





