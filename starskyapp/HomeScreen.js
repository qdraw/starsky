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

import { NavigationActions } from 'react-navigation'

// import folderfetch from './folderfetch';


export default class ArchiveScreen extends React.Component {
  constructor(props){
    super(props);
    const { params } = this.props.navigation.state;
    this.state = { 
       isLoading: true,
       filePath: params ? params.filePath : "/"
    }
  }
  
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

          // var resetAction = NavigationActions.reset({
          //   index: 0,
          //   actions: []
          // });
          
          // responseJson.breadcrumb.forEach(breadItem => {
          //   resetAction.actions.push(
          //     NavigationActions.navigate({ routeName: 'Home'
          //   }));
          //   resetAction.index = resetAction.index+1
          // });

          // console.log("resetAction");

          // console.debug(resetAction);
          // this.props.navigation.push('Details',{
          //   filePath: this.state.filePath
          // });

          // this.props
          //   .navigation
          //   .dispatch(resetAction);



          // this.props
          //   .navigation
          //   .dispatch(NavigationActions.reset(
          //     {
          //       index: 0,
          //       actions: [
          //         NavigationActions.navigate({ routeName: 'Home'})
          //       ]
          //     }));
          // // this.props.navigation
          // // // this.props.navigation.goBack()


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





