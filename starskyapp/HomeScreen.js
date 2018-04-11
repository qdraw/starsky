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
  Button,
  Dimensions
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

  _renderScene() {

    if (this.state.dataSource.pageType === 'Archive') {
      return(
        <View style={{flex: 1, paddingTop:20}}>
  
          <FlatList
            data={this.state.dataSource.fileIndexItems}
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
    if (this.state.dataSource.pageType === 'DetailView') {
      var thumbPath = 'http://localhost:5000/api/thumbnail?f=' + this.state.dataSource.fileIndexItem.fileHash
      return(
          <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
          <Text>Details Screen</Text>
          <Text>otherParam: {JSON.stringify(this.state.dataSource.fileIndexItem.filePath)}</Text>
          <Text>otherParam: {thumbPath}</Text>

          <Image
            style={styles.stretch}
            source={{uri: thumbPath}}
          />

          <Button
            title="Update the title"
            onPress={() =>
              this.props.navigation.setParams({ otherParam: 'Updated!' })}
          />
          <Button
            title="Go to Details... again"
            onPress={() => this.props.navigation.navigate('Details')}
          />
          <Button
            title="Go back"
            onPress={() => this.props.navigation.goBack()}
          />
        </View>
      )
    }

 }
  
  componentDidMount(){

    return fetch('http://localhost:5000/?json=true&f=' + this.state.filePath)
      .then((response) => response.json())
      .then((responseJson) => {

        this.setState({
          isLoading: false,
          dataSource: responseJson,
        }, function(){
        });

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

    return this._renderScene()

  }
}

var styles = StyleSheet.create({
  stretch: {
    width: Dimensions.get('window').width,
    height: 300,
    backgroundColor: '#ededed',
    marginTop: 10,
    justifyContent: 'center',
    alignItems: 'center'
  }
});






